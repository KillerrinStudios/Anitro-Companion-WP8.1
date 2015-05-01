using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures
{
    public enum APIResponse
    {
        None,
        Successful,
        Failed,
        
        //-- Standard Errors
        APIError,
        NetworkError,
        ServerError,
        UnknownError,

        //-- Specialized Errors
        NotSupported,

        // Login
        InfoNotEntered,
        InvalidCredentials,

        //
    }
}
