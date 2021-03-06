﻿//-----------------------------------------------------------------------
// <copyright file="JsonExtensions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
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
// </license>

namespace System.IdentityModel.Tokens
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Script.Serialization;
    
    /// <summary>
    /// Dictionary extensions for serializations
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Suppressed for private fields.")]
    internal static class JsonExtensions
    {
        public static string SerializeToJson(this object value)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(value);
        }

        public static T DeserializeFromJson<T>(this string value)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(value);
        }

        public static JwtHeader DeserializeJwtHeader(this string value)
        {
            return DeserializeFromJson<JwtHeader>(value);
        }

        public static JwtPayload DeserializeJwtPayload(this string value)
        {
            return DeserializeFromJson<JwtPayload>(value);
        }
    }
}
