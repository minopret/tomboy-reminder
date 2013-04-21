//
// MagicUtils.cs: Convenient methods for MagicString classes
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

// $Id: MagicUtils.cs,v 1.1 2005/04/11 11:29:30 kikidonk Exp $

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace MagicString {
	public class NoMagicMatchesException : Exception {
		public NoMagicMatchesException(string s) : base(s) {}
	}
		
	public class DateTimeWrapper {
		private DateTime date;
		public DateTimeWrapper(DateTime date) {
			this.date = date;
		}
		
		public DateTime Date {
			get { return date; }
		}
	}
	
	public class ParseResult {
		private DateTime dt;
		private bool hasTime = false;
		private bool hasDate = false;
		
		public bool HasTime {
			get { return hasTime; }
			set { hasTime = value; }
		}
		
		public bool HasDate {
			get { return hasDate; }
			set { hasDate = value; }
		}
		
		public DateTime Date {
			get { return dt; }
			set { dt = value; }
		}
	}
}
