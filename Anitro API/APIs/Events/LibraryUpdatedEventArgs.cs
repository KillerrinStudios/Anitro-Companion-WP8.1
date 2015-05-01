using Anitro.Data_Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.APIs.Events
{
    public delegate void LibraryUpdatedEventHandler(object sender, LibraryUpdatedEventArgs e);

    public class LibraryUpdatedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        public APIResponse Result { get; private set; }
        public APIType Type { get; private set; }

        public LibraryUpdatedEventArgs()
            : base(new Exception(), false, null)
        {
            Result = APIResponse.None;
            Type = APIType.None;
        }
        public LibraryUpdatedEventArgs(APIResponse _result, APIType _type)
            : base(new Exception(), false, null)
        {
            Result = _result;
            Type = _type;
        }
        public LibraryUpdatedEventArgs(APIResponse _result, APIType _type, Exception e, bool canceled, Object state)
            : base(e, canceled, state)
        {
            Result = _result;
            Type = _type;
        }
    }
}
