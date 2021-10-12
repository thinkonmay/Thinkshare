#include <remote-app-input.h>
#include <gst/gst.h>
#include <remote-app-type.h>
#include <remote-app-data-channel.h>


#include <gst\video\navigation.h>

#include <human-interface-opcode.h>
#include <message-form.h>




struct _HidInput
{
    gdouble x_pos;
    gdouble y_pos;

    gdouble delta_x;
    gdouble delta_y;

    gint button_code;
    gchar* keyboard_code;

    HidOpcode* opcode;

    JsonObject* json;
};

static void
send_mouse_move_signal(HidInput* input,
                  RemoteApp* core)
{
    JsonObject* object = json_object_new();
    json_object_set_int_member(object,"Opcode",(gint)input->opcode);
    json_object_set_int_member(object,"dX",(gint)input->x_pos);
    json_object_set_int_member(object,"dY",(gint)input->y_pos);
    hid_data_channel_send(get_string_from_json_object(object),core);
}

static void
send_mouse_button_signal(HidInput* input,
                         RemoteApp* core)
{
    JsonObject* object = json_object_new();
    json_object_set_int_member(object,"Opcode",(gint)input->opcode);
    json_object_set_int_member(object,"button",input->button_code);
    json_object_set_int_member(object,"dX",(gint)input->x_pos);
    json_object_set_int_member(object,"dY",(gint)input->y_pos);
    hid_data_channel_send(get_string_from_json_object(object),core);
}


static void
send_key_event(HidInput* input,
            RemoteApp* core)
{
    JsonObject* object = json_object_new();
    json_object_set_int_member(object,"Opcode",(gint)input->opcode);
    json_object_set_string_member(object,"wVk",input->keyboard_code);
    hid_data_channel_send(get_string_from_json_object(object),core);
}

static void
parse_hid_event(HidInput* input, 
                RemoteApp* core)
{
    switch((gint)input->opcode)
    {
        case MOUSE_MOVE:
            send_mouse_move_signal(input,core);
            break;
        case MOUSE_UP:
            send_mouse_button_signal(input,core);
            break;
        case MOUSE_DOWN:
            send_mouse_button_signal(input,core);
            break;
        case MOUSE_WHEEL:
            break;
        case KEYUP:
            send_key_event(input,core);
            break;
        case KEYDOWN:
            send_key_event(input,core);
            break;
        default:
            return;
    }
}


gboolean      
handle_navigator(GstEvent *event, 
                RemoteApp* core)
{
    HidInput* navigation = malloc(sizeof(HidInput));
    gint eventcode = gst_navigation_event_get_type(event);\
    
    switch (eventcode)
    {
        case GST_NAVIGATION_EVENT_KEY_PRESS: 
            gst_navigation_event_parse_key_event(event,&(navigation->keyboard_code));
            navigation->opcode = KEYDOWN;
            break; 
        case GST_NAVIGATION_EVENT_KEY_RELEASE: 
            gst_navigation_event_parse_key_event(event,&(navigation->keyboard_code));
            navigation->opcode = KEYUP;
            break;
        case GST_NAVIGATION_EVENT_MOUSE_MOVE: 
            gst_navigation_event_parse_mouse_move_event(event,&(navigation->x_pos),&(navigation->y_pos));
            navigation->opcode = MOUSE_MOVE;
            break; 
        case GST_NAVIGATION_EVENT_MOUSE_SCROLL: 
            gst_navigation_event_parse_mouse_scroll_event(event,&(navigation->x_pos),&(navigation->y_pos),&(navigation->delta_x),&(navigation->delta_y));
            navigation->opcode = MOUSE_WHEEL;
            break; 
        case GST_NAVIGATION_EVENT_MOUSE_BUTTON_PRESS: 
            gst_navigation_event_parse_mouse_button_event(event,&(navigation->button_code),&(navigation->x_pos),&(navigation->y_pos));
            navigation->opcode = MOUSE_DOWN;
            break; 
        case GST_NAVIGATION_EVENT_MOUSE_BUTTON_RELEASE: 
            gst_navigation_event_parse_mouse_button_event(event,&(navigation->button_code),&(navigation->x_pos),&(navigation->y_pos));
            navigation->opcode = MOUSE_UP;
            break; 
        default:
            break;
    }
    parse_hid_event(navigation,core);
    free(navigation);
}