using System;
using System.Collections.Generic;
using System.Text;

using Anitro.Data_Structures;

namespace Anitro.APIs.Events
{
    public delegate void APICompletedEventHandler(object sender, APICompletedEventArgs e);

    public class APICompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        public APIResponse Result { get; private set; }
        public APIType Type { get; private set; }
        public Object ResultObject { get; private set; }

        public APICompletedEventArgs()
            : base(new Exception(), false, null)
        {
            Result = APIResponse.None;
            Type = APIType.None;
            ResultObject = null;
        }
        public APICompletedEventArgs(APIResponse _result, APIType _type)
            : base(new Exception(), false, null)
        {
            Result = _result;
            Type = _type;
            ResultObject = null;
        }
        public APICompletedEventArgs(APIResponse _result, APIType _type, Object _resultObject)
            : base(new Exception(), false, null)
        {
            Result = _result;
            Type = _type;
            ResultObject = _resultObject;
        }
        public APICompletedEventArgs(APIResponse _result, APIType _type, Exception e, bool canceled, Object state)
            :base(e, canceled, state)
        {
            Result = _result;
            Type = _type;
        }
    }
}
