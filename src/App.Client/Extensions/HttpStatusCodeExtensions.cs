﻿using System.Net;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccess(this HttpStatusCode me) => (int)me >= 200 && (int)me < 300;
}
