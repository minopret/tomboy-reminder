//
// MagicTime.cs: Parses natural langage times.
//
// Copyright 2005,  Raphaël Slinckx <raphael@slinckx.net>
//					Yoshua XXXX
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

// $Id: MagicTime.cs,v 1.1 2005/04/11 11:29:30 kikidonk Exp $

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace MagicString {
	public class MagicTime {
		private static string TIME_SEP = ":|h|\\W+";
		
		private static Regex[] TIMES = {
			// 12:00 am | 10 h am | 5pm
			new Regex("^(?<hour>[1-9]|1[0-2])\\W*("+TIME_SEP+")\\W*(?<min>[0-5][0-9])?\\W*(?<ampm>[ap]m)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			// 23:14 | 13h
			new Regex("^(?<hour>[0-9]|1[0-9]|2[0-3]|00)\\W*("+TIME_SEP+")\\W*(?<min>[0-5][0-9])?$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		};
	    
		public DateTime AddTime(string time, DateTime baseDate) {
			time = time.Trim();
			
			DateTimeWrapper result;
			
			//Parse a regular date
			foreach(Regex regex in TIMES) {
				result = Match(regex, time, baseDate);
				if(result != null)
					return result.Date;
			}
			
			throw new NoMagicMatchesException("No matches found in string '"+time+"'");
		}
	    	    
	    private DateTimeWrapper Match(Regex regex, string date, DateTime baseDate) {
	    	Match match = regex.Match (date);
			if(match.Success) {
				int hour = Int32.Parse (match.Groups["hour"].ToString());

				string mins = match.Groups["min"].ToString();
				int min = 0;
				if(mins != "")
					min = Int32.Parse (mins);
			
				string ampm = match.Groups["ampm"].ToString();
				if(ampm != "") {
					/* Got a ampm date format */
					if (hour == 12)
						hour = 0;
					
					if (String.Compare(ampm, "pm", true) == 0)
						hour += 12;
				}
				
				return new DateTimeWrapper (baseDate.AddHours (hour).AddMinutes (min));
			}
			
			return null;
		}
		
		/*
		public static void Main (string[] args)
		{
			MagicTime mt = new MagicTime();
			DateTime now = DateTime.Today;
			Console.WriteLine(mt.AddTime(args[0], now));
		}
		*/
	}
}
