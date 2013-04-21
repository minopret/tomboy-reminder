//
// MagicParser.cs: Convenient methods for MagicDate+Time
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

// $Id: MagicParser.cs,v 1.1 2005/04/11 11:29:30 kikidonk Exp $

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace MagicString {
	public class MagicParser {
		private MagicDate md = new MagicDate();
		private MagicTime mt = new MagicTime();
		
		private static string DATE_TIME_SEP = "at|a|à|@";
		
		private static Regex[] GENERIC = {
			new Regex("^(?<date>.*)\\W+("+DATE_TIME_SEP+")\\W+(?<time>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^(?<date>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		};
		
		public ParseResult Parse(string s, DateTime baseDate) {
			s = s.Trim();
			
			ParseResult result;
			
			//Parse a regular date
			foreach(Regex regex in GENERIC) {
				result = Match(regex, s, baseDate);
				if(result != null)
					return result;
			}
			
			throw new NoMagicMatchesException("No matches found in string '"+s+"'");
		}
		
		private ParseResult Match(Regex regex, string s, DateTime baseDate) {
			ParseResult parseResult = null;
			
	    	Match match = regex.Match (s);
			if(match.Success) {
				string date = match.Groups["date"].ToString();
				string time = match.Groups["time"].ToString();
				
				parseResult = new ParseResult ();
				
				DateTime dateTime = baseDate;
				if(date != "" && time != "") {
					dateTime = md.ParseDate (date, baseDate);
					dateTime = mt.AddTime (time, dateTime);
					
					parseResult.HasTime = true;
					parseResult.HasDate = true;
				}
				else if(date != "" && time == "") {
					//either a date only or a time only
					try {
						dateTime = 	md.ParseDate (date, baseDate);
						parseResult.HasDate = true;
					} catch (Exception e) {
						dateTime = mt.AddTime(date, baseDate);
						parseResult.HasTime = true;
					}
				}
				
				parseResult.Date = dateTime;
			}
			
			return parseResult;
		}
		
		public static void Main (string[] args)
		{
			MagicParser mp = new MagicParser ();
			Console.WriteLine (mp.Parse(args[0], DateTime.Today));
		}
	}
}
