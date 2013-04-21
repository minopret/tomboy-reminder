//
// Reminder.cs: A tomboy plugin that displays a note at specified dates.
//
// Copyright 2005,  Raphaël Slinckx <raphael@slinckx.net>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

// $Id: Reminder.cs,v 1.4 2005/05/11 11:59:23 kikidonk Exp $

using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.InteropServices;

using Gtk;

using Tomboy;

using MagicString;

namespace Tomboy.Reminder
{
	public class Reminder : NoteAddin
	{
		//The reminder date expression, currently (in a non-langage specific fashion) it is
		// !date
		//or
		// remind date | alert: next monday | rappel mardi prochain
		//anywhere once in a line
		static string[] REMINDER_PATTERNS = {
			".*\\!",
			"(remind|rappel|alert)\\W+"
		};
		
		static Regex REMINDER =	new Regex ("^(("+
			REMINDER_PATTERNS[0]
			+")|("+
			REMINDER_PATTERNS[1]
			+"))(?<date>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
		
		static MagicParser magicParser = new MagicParser();
		
		static Hashtable noteEvents = new Hashtable();
		
		static uint RE_REMIND_DELAY = 30*60; //Wait x before re-showing [in seconds] (snooze)
		
		public override void Initialize ()
		{
			// If a tag of this name already exists, don't install.
			if (Note.TagTable.Lookup ("reminder") == null) {
				TextTag highlightTag = new TextTag("reminder");
				highlightTag.Background = "yellow";
				
				Note.TagTable.Add (highlightTag);
			}
			
			ScanNote();
		}
				
		public override void Shutdown () {
			//Remove the delegate
			Window.UnmapEvent -= OnWindowClosed;
		}
		
		public override void OnNoteOpened () 
		{
			//Add a delegate when the user closes the window
			Window.UnmapEvent += OnWindowClosed;
			
			// Watch text changes to highlight valid patterns	
			Buffer.InsertText += OnInsertText;
			Buffer.DeleteRange += OnDeleteRange;

			ScanNote();
		}
			
		void OnWindowClosed (object sender, UnmapEventArgs args)
		{		
			//Parse the note to search for dates
			ScanNote();
		}

		void OnInsertText (object sender, Gtk.InsertTextArgs args)
		{
			SetupTimer(Note, args.Pos, args.Pos);
		}

		void OnDeleteRange (object sender, Gtk.DeleteRangeArgs args)
		{
			SetupTimer(Note, args.Start, args.End);
		}
		
		void SetupTimer(Note note, TextIter start, TextIter end)
		{
			if (!start.StartsLine())
				start.BackwardLine();
			if (!end.EndsLine())
				end.ForwardToLineEnd ();

			Buffer.RemoveTag("reminder", start, end);
			//Buffer.RemoveAllTags(start, end); // This breaks stuff - what purpose does it serve?
			SetupTimer(Note, start.GetSlice(end), start.Line);
		}
		
		protected void ScanNote()
		{
			//Delete every event associated with this note
			RemoveEvents(Note);
			
			string[] lines = Note.TextContent.Split('\n');
			int i = 0;
			foreach(string line in lines) {
				SetupTimer(Note, line, i++);
			}
		}
		
		private void RemoveEvents(Note note)
		{
			IList events = noteEvents[note] as IList;
			if(events == null)
				return;
				
			foreach(ReminderEvent ev in events) {
				ev.Cancel ();
			}
			noteEvents.Remove(note);
		}
		
		private void AddEvent (ReminderEvent ev) {
			//Retreive the events for this note
			IList events = noteEvents[ev.Note] as IList;
			if(events == null) {
				//No previous events for this note, create a new list
				events = new ArrayList();
				noteEvents[ev.Note] = events;
			}
			else if(events.Contains(ev))
				return;
			
			//Add the new events fo rthe note
			events.Add(ev);
			
			ev.Start();
		}
		
		private void HighlightEvent (Note note, int i)
		{
			NoteBuffer buf = note.Buffer;
			Console.WriteLine("Highlight line:"+i);
			Gtk.TextIter start = buf.GetIterAtLine(i);
			Gtk.TextIter end = start;
			end.ForwardToLineEnd();
			
			buf.ApplyTag ("reminder", start, end);
		}
		
		private void SetupTimer (Note note, string line, int i)
		{
			//Console.WriteLine("Reminder.cs:line:"+line);
			for (Match match = REMINDER.Match (line); match.Success; match = match.NextMatch ()) {
				ParseResult result;
				try {
					//Console.WriteLine("Reminder.cs:Parsing date:"+match.Groups ["date"].ToString());
					result = magicParser.Parse(match.Groups ["date"].ToString(), note.ChangeDate.Date);
				} catch(Exception e) {
					continue;
				}

				DateTime time = result.Date;	

				DateTime now = DateTime.Now;
				long due = (time.Ticks - now.Ticks)/10000;
				
				//Console.WriteLine("Reminder.cs:Got Reminder:"+now+" (due in: "+due+")");
				if(!result.HasTime && result.HasDate) {
					//The reminder was just a date, with no specified time, we remind the note
					//through the entire day
					if(time.Day == now.Day && time.Month == now.Month && time.Year == now.Year) {
						due = RE_REMIND_DELAY*1000; //Wait some time, before re-showing again
					}
				}
				
				//We have a past reminder, forget it.
				if(due < 0)
					continue;

				Console.WriteLine("Reminder.cs:Adding Reminder:"+time+" (due in: "+due/1000+" sec)");			
				AddEvent(new ReminderEvent(note, time, due));
				HighlightEvent(note, i);
			}
		}
		
		private class ReminderEvent {
			private Note note;
			private DateTime date;
			private long dueTime;
			private bool cancelled = false;
			
			public ReminderEvent(Note note, DateTime date, long dueTime) {
				this.note = note;
				this.date = date;
				this.dueTime = dueTime;
			}
			
			public void Start () {
				Console.WriteLine("Reminder.cs:Timer '"+note.Title+"' @ "+date);
				GLib.Timeout.Add((uint) dueTime, new GLib.TimeoutHandler(OnShowReminder));
			}
			
			public void Cancel () {
				cancelled = true;
			}
			
			[DllImport("libgtk-x11-2.0.so.0")]
			static extern void gtk_window_set_keep_above (IntPtr raw, bool setting);
			
			private bool OnShowReminder ()
			{
				if(! cancelled) {
					(noteEvents[note] as IList).Remove(this);
					Console.WriteLine("Reminder.cs:New Reminder: {0}", note.Title);
					
					Window w = note.Window;
					
					// Try to use the gtk lib to set the window on top
					try {
						gtk_window_set_keep_above (w.Handle, true);
					} catch (Exception e) {
						//Console.WriteLine ("Could not set on top: {0}", e);
					}
					
					w.Stick ();
					w.Present ();
				}
				
				return false;
			}
					
			public Note Note {
				get { return note; }
			}
			
			public DateTime Date {
				get { return date; }
			}
			
			public override bool Equals (object o) {
				ReminderEvent ev = o as ReminderEvent;
				return note.Equals(ev.Note) && date == ev.Date;
			}
			
			public override int GetHashCode () {
				return note.GetHashCode () ^ date.GetHashCode ();
			}
		}
	}
}