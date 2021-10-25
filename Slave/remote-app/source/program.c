#include <remote-app.h>
#include <remote-app-type.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>

#include <string-spliter.h>
#include <general-constant.h>

#ifndef GST_USE_UNSTABLE_API
#define GST_USE_UNSTABLE_API
#endif

static gchar signalling_url[50] = "wss://signalling.thinkmay.net/Session";
static gchar turn[100] = "turn://thinkmaycoturn:thinkmaycoturn_password@turn:stun.thinkmay.net:3478";
static gint  session_id = 0;
static gchar video_codec[50] = {0};
static gchar audio_codec[50] = {0}; 
static gchar connection_string[200] = {0};


#define GST_DEBUG               4

static GOptionEntry entries[] = {
  {"sessionid", 0, 0, G_OPTION_ARG_INT, &session_id,
      "String ID of the peer to connect to", "ID"},
  {"signalling", 0, 0, G_OPTION_ARG_STRING, &signalling_url,
      "Signalling server to connect to", "URL"},
  {"turn", 0, 0, G_OPTION_ARG_STRING, &turn,
      "Request that the peer generate the offer and we'll answer", "URL"},
  {"audiocodec", 0, 0, G_OPTION_ARG_STRING, &audio_codec,
      "audio codec use for decode bin", "codec"},
  {"videocodec", 0, 0, G_OPTION_ARG_STRING, &video_codec,
      "video codec use for decode bin", "codec"},
  {"connection", 0, 0, G_OPTION_ARG_STRING, &connection_string,
      "connection ", "codec"},
  {NULL},
};


char**
split(char *string, 
      const char delimiter) 
{
    int length = 0, count = 0, i = 0, j = 0;
    while(*(string++)) {
        if (*string == delimiter) count++;
        length++;
    }
    string -= (length + 1); // string was incremented one more than length
    char **array = (char **)malloc(sizeof(char *) * (length + 1));
    char ** base = array;
    for(i = 0; i < (count + 1); i++) {
        j = 0;
        while(string[j] != delimiter) j++;
        j++;
        *array = (char *)malloc(sizeof(char) * j);
        memcpy(*array, string, (j-1));
        (*array)[j-1] = '\0';
        string += j;
        array++;
    }
    *array = '\0';
    return base;  
}

void
string_split_free(gchar** base)
{
    gint i = 0;
    while(base[i]) {
        free(base[i]);
        i++;
    }
    free(base);
    base = NULL;
}


int
main(int argc, char* argv[])
{
    thinkmay_init(argv[0],19);
    if(strlen(connection_string))
    {
        gchar** url_field = string_spliter(connection_string,"/");
        gchar* parameter_field = (url_field+2);
        gchar** parameter_array = string_pliter(parameter_field,".");
        while(*(parameter_array++))
        {
            
        }
    }

    GOptionContext *context;
    GError *error = NULL;

    context = g_option_context_new ("- thinkmay gstreamer client");
    g_option_context_add_main_entries (context, entries, NULL);
    g_option_context_add_group (context, gst_init_get_option_group ());
    if (!g_option_context_parse (context, &argc, &argv, &error)) {
        g_printerr ("Error initializing: %s\n", error->message);
        return -1;
    }

    if(argc == 2)
    {
        gchar** array = split(argv[1],'/');

        if(g_strcmp0(array[0],"thinkmay:"))
        {
            g_print("%s :",array[0]);
            g_printerr("wrong uri, remote app exiting");
            return;
        }


        gchar** array_param = split(array[2],'.');
        do
        {
            if(*(array_param))
            {
                gchar** parameter = split(*(array_param),'=');
                if(!g_strcmp0(*(parameter ),"sessionid"))
                {
                    gint id = strtol(*(parameter +1),NULL,10);
                    session_id = id;
                }
                else if(!g_strcmp0(*(parameter ),"signalling"))
                {
                    memcpy(signalling_url,*(parameter +1),strlen(*(parameter +1)));
                }
                else if(!g_strcmp0(*(parameter ),"turn"))
                {
                    memcpy(turn,*(parameter +1),strlen(*(parameter +1)));
                }
                else if(!g_strcmp0(*(parameter ),"videocodec"))
                {
                    memcpy(video_codec,*(parameter +1),strlen(*(parameter +1)));
                }
            }
        }
        while(*(array_param++));
    }

    g_print("Starting connnection with session client id %d, videocodec %s , signalling server url %s\n",session_id,video_codec,signalling_url);



    remote_app_initialize(session_id,
                    signalling_url, 
                    turn, 
                    audio_codec, 
                    video_codec);
    return 0;
}
