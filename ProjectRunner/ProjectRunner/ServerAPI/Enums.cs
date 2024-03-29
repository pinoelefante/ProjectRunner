﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectRunner.ServerAPI
{
    public enum StatusCodes
    {
        METHOD_NOT_IMPLEMENTED = -1000,
        SERVER_ERROR = -500, //errore presente solo sul client
        CONNECTION_ERROR = -501, //errore presente solo sul client

        ENVELOP_UNSET = 0,

        FAIL = -1,
        RICHIESTA_MALFORMATA = -2,
        METODO_ASSENTE = -3,
        SQL_FAIL = -4,

        OK = 1,

        LOGIN_ERROR = 10,
        LOGIN_GIA_LOGGATO = 11,
        LOGIN_NON_LOGGATO = 12,

    }
    public enum PushDevice
    {
        NOT_SET = 0,
        ANDROID = 1,
        IOS = 2,
        WINDOWS_UWP = 3
    }
    public enum FriendshipStatus
    {
        USER_ACCOUNT = 0,
        IS_FRIEND = 1,
        REQUESTED = 2,
        RECEIVED = 3,
        STRANGER = 4
    }
}
