using System;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace MineProxy.Network
{
    public class UsernameUUID
    {
        const string url = "https://api.mojang.com/profiles/page/1";

        class Request
        {
            public string name { get; set; }

            public string agent { get; set; }

            public Request()
            {
                agent = "minecraft";
            }
        }

        class Response
        {
            public List<Profile> profiles { get; set; }

            public class Profile
            {
                public string username { get; set; }

                public Guid id { get; set; }
            }
        }

        /// <summary>
        /// Retrieve the UUID given a username
        /// This one is no longer used during auth since the new method get the UUID
        /// </summary>
        public static Guid GetUUID(string username)
        {
            using (WebClient client = new WebClient())
            {
                /*
            'header'  => "Content-type: application/json\r\n",
            'method'  => 'POST',
            'content' => '{"name":"'.$username.'","agent":"minecraft"}',

                context  = stream_context_create(options);
                result = file_get_contents(url, false, $context);
                return res;
                */

                var request = new Request();
                request.name = username;

                byte[] req = Json.Serialize(request);

                // Download data.
                byte[] resp = client.UploadData(url, req);
                var response = Json.Deserialize<Response>(resp);

                if(response.profiles.Count == 0)
                    throw new InvalidOperationException("Bad response: " + Encoding.UTF8.GetString(resp));
                Guid id = response.profiles[0].id;
                if (id == Guid.Empty)
                    throw new InvalidOperationException("Bad response: " + Encoding.UTF8.GetString(resp));
                return id;
            }
        }

    }
}

