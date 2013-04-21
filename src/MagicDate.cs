//
// MagicDate.cs: Parses natural langage dates.
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
// Adapted in part from :
// 'Magic' date parsing, by Simon Willison (6th October 2003)
// http://simon.incutio.com/archive/2003/10/06/betterDateInput
//

// $Id: MagicDate.cs,v 1.2 2005/04/11 11:29:05 kikidonk Exp $

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace MagicString {
	public class MagicDate {

		private static Hashtable MONTH_NUMBERS = new Hashtable();
		private static string MONTH_PREFIXES = "jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|" + //English
									   "jan|fev|mar|avr|mai|jui|jui|aou|sep|oct|nov|dec|" + //French
									   "янв|фев|мар|апр|май|июн|июл|авг|сеп|окт|нов|дек";//Russian
		
		private static Hashtable DAY_NUMBERS = new Hashtable();
		private static string DAY_PREFIXES = "mon|tue|wed|thu|fri|sat|sun|" + //English
			    					 "lun|mar|mer|jeu|ven|sam|dim|" + //French
			    					 "пн|вт|ср|чт|пт|сб|вс"; //Russian
									   							   
		private static string DAYS_PARTICLE = "st|nd|rd|th";
		private static string NEXT = "next|suivant|prochain";
		private static string TODAY = "(today)|(aujourd.?...)";
		
		private static Regex[] GENERIC = {
		   	//4th | 2nd | 3rd | 1st | 10th | 2 | 32
	    	new Regex ("^(?<daynb>\\d{1,2})("+DAYS_PARTICLE+")?$", RegexOptions.IgnoreCase | RegexOptions.Compiled ),
	    	//4th Jan | 4 Jan | 4 january | 4th,january 
	    	new Regex ("^(?<daynb>\\d{1,2})(?:"+DAYS_PARTICLE+")?\\W+(?<monthname>\\w{3,})$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	//4th Jan 2003 | 4 jan 03
	    	new Regex ("^(?<daynb>\\d{1,2})(?:"+DAYS_PARTICLE+")?\\W+(?<monthname>\\w{3,})\\W+(?<yearnb>\\d{2}\\d{2}?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	//Jan 4th
	    	new Regex ("^(?<monthname>\\w{3,})\\W+(?<daynb>\\d{1,2})(?:"+DAYS_PARTICLE+")?$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	// Jan 4th 2003
	    	new Regex ("^(?<monthname>\\w{3,})\\W+(?<daynb>\\d{1,2})(?:"+DAYS_PARTICLE+")?\\W+(?<yearnb>\\d{2}\\d{2}?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	// yyyy-mm-dd | yyyy/mm/dd | yy mm dd
	    	new Regex ("^(?<yearnb>\\d{2}\\d{2}?)\\W(?<monthnb>\\d{1,2})\\W(?<daynb>\\d{1,2})$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
	    };
	    
	    private static Regex[] RELATIVE = {
	    	// today
	    	new Regex ("^(?<today>"+TODAY+")$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	
	    	// next day_of_week | week | 
	    	new Regex ("^(?<next>"+NEXT+")\\W+(?<day>\\w{3,})$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
	    	new Regex ("^(?<day>\\w{3,})\\W+(?<next>"+NEXT+")$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
	    };
	    
	    public MagicDate () {
	    	//Store month name/index
	    	string[] months = MONTH_PREFIXES.Split('|');
	    	int i = 0;
	    	foreach(string month in months) {
	    		MONTH_NUMBERS[month] = (i++ % 12) + 1;
	    	}
	    	
	    	string[] days = DAY_PREFIXES.Split('|');
	    	i = 0;
	    	foreach(string day in days) {
	    		DAY_NUMBERS[day] = (i++ % 7);
	    	}
		}
		
		public DateTime ParseDate(string date, DateTime baseDate) {
			date = date.Trim();
			
			DateTimeWrapper result;	
			
			//Parse a regular date
			foreach(Regex regex in GENERIC) {
				result = Match(regex, date, baseDate);
				if(result != null)
					return result.Date;
			}
			
			//Parse a reltive date
			foreach(Regex regex in RELATIVE) {
				result = MatchRelative(regex, date, baseDate);
				if(result != null)
					return result.Date;
			}
			
			throw new NoMagicMatchesException("No date found in string '"+date+"'");
		}
		
		public DateTime ParseDate(string date) {
			return ParseDate(date, DateTime.Now);
	    }
	    
	    private DateTimeWrapper MatchRelative(Regex regex, string date, DateTime baseDate) {
	    	Match match = regex.Match (date);
			if(match.Success) {
				int month = baseDate.Month;
				int year = baseDate.Year;
				int day = baseDate.Day;
				
				if (match.Groups["today"].Success) {
					return new DateTimeWrapper (new DateTime (year, month, day));
				}
				
				DayOfWeek dayofweek = baseDate.DayOfWeek;
				int daynb;
				switch(dayofweek) {
					case DayOfWeek.Monday:    daynb = 0; break;
					case DayOfWeek.Tuesday:   daynb = 1; break;
					case DayOfWeek.Wednesday: daynb = 2; break;
					case DayOfWeek.Thursday:  daynb = 3; break;
					case DayOfWeek.Friday:    daynb = 4; break;
					case DayOfWeek.Saturday:  daynb = 5; break;
					case DayOfWeek.Sunday:    daynb = 6; break;
					default: daynb = -1;break;
				}
				
				int parsedNb = ParseDay( match.Groups["day"].ToString() );
				if(parsedNb == -1)
					return null;
					
				int deltaDays = daynb - parsedNb;
				deltaDays = (deltaDays < 0) ? -deltaDays : (6-deltaDays)+1;
					
				return new DateTimeWrapper (new DateTime (year, month, day).AddDays(deltaDays));
			}
			
			return null;
	    }
	    
	    private DateTimeWrapper Match(Regex regex, string date, DateTime baseDate) {
	    	Match match = regex.Match (date);
			if(match.Success) {
				int month = baseDate.Month;
				int year = baseDate.Year;
				
				int day = Int32.Parse (match.Groups["daynb"].ToString());
				
				string monthstring = match.Groups["monthname"].ToString();
				if(monthstring != "") {
					month = ParseMonth(monthstring);
					if(month == -1)
						return null;
				}
				else if((monthstring = match.Groups["monthnb"].ToString()) != "") {
					month = Int32.Parse (monthstring);
				}
				
				string yearstring = match.Groups["yearnb"].ToString();
				if(yearstring != "") {
					year = Int32.Parse (yearstring);
				}
				
				return new DateTimeWrapper (new DateTime (year, month, day));
			}
			
			return null;
		}
		
		private string ReplaceAccents(string s) {
			return s.Replace("é", "e").Replace("ô", "o");
		}
		
		private int ParseMonth(string month) {
			month = ReplaceAccents(month.Substring(0, 3));
			object monthnb = MONTH_NUMBERS[month.ToLower()];
			if(monthnb == null)
				return -1;
			
			return (int) monthnb ;
		}
		
		private int ParseDay(string day) {
			day = day.Substring(0, 3);
			object daynb = DAY_NUMBERS[day.ToLower()];
			if(daynb == null)
				return -1;
			
			return (int) daynb ;
		}
		
		/*
		public static void Main (string[] args)
		{
			MagicDate md = new MagicDate();
			Console.WriteLine(md.ParseDate(args[0]));
		}
		*/
	}
}
