#ifndef __AGENT_OBJECT_H__
#define __AGETN_OBJECT_H__

#include <glib-object.h>
#include <glib.h>
#include <agent-type.h>
#include <agent-device.h>
 
G_BEGIN_DECLS

/// <summary>
/// state of SessionCore (used to report session core bug to host)
/// </summary>
typedef enum
{
	APP_STATE_UNKNOWN = 0,
	APP_STATE_ERROR = 1,          /* generic error */
	SERVER_CONNECTING = 1000,
	SERVER_CONNECTION_ERROR,
	SERVER_CONNECTED,             /* Ready to register */
	SERVER_REGISTERING = 2000,
	SERVER_REGISTRATION_ERROR,
	SERVER_REGISTERED,            /* Ready to call a peer */
	SERVER_CLOSED,                /* server connection closed by us or the server */
	PEER_CONNECTING = 3000,
	PEER_CONNECTION_ERROR,
	PEER_CONNECTED,
	PEER_CALL_NEGOTIATING = 4000,
	PEER_CALL_STARTED,
	PEER_CALL_STOPPING,
	PEER_CALL_STOPPED,
	PEER_CALL_ERROR,
	SESSION_DENIED,
	SESSION_INFORMATION_SETTLED,
	WAITING_SESSION_INFORMATION,
	PIPELINE_SETUP_DONE,
	DATA_CHANNEL_CONNECTED,
	HANDSHAKE_SIGNAL_CONNECTED,

	CORE_STATE_LAST
}CoreState;



#define AGENT_TYPE_OBJECT agent_object_get_type()
/*declare derivable enable agentobject to have virtual method*/
G_DECLARE_DERIVABLE_TYPE (AgentObject, agent_object, AGENT ,OBJECT, GObject)


/// <summary>
/// base class for agent object
/// </summary>
struct _AgentObjectClass
{
	GObjectClass parent;

	gboolean 			(*connect_to_host)			(AgentObject* self);

	gboolean			(*disconnect_host)			(AgentObject* self);

	gboolean			(*session_initializate)		(AgentObject* self);

	gboolean			(*session_terminate)		(AgentObject* self);

	gboolean			(*remote_control_disconnect)(AgentObject* self);

	gboolean			(*remote_control_reconnect)	(AgentObject* self);

	gboolean			(*command_line_passing)		(AgentObject* self,
													gchar* command);

	gboolean			(*add_local_nas_storage)	(AgentObject* self, 
													LocalStorage* storage);

	void				(*send_message)				(AgentObject* self,
													Message* message);
};

/// <summary>
/// Create new agent object based on information of host
/// </summary>
/// <param name="Host_URL"></param>
/// <param name="Host_ID"></param>
/// <returns></returns>
AgentObject*									agent_object_new					(gchar* Host_URL);

/// <summary>
/// handle message from host
/// </summary>
/// <param name="self"></param>
void											handle_host_connection				(AgentObject* self);

/// <summary>
/// handle message from session core and session loader
/// </summary>
/// <param name="self"></param>
void											handle_shared_memory_hub			(AgentObject* self);

/// <summary>
/// send message to arbitrary device in system
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
gboolean										send_message						(AgentObject* self,
																					Message* message);


IPC*											agent_object_get_ipc				(AgentObject* self);


Socket*											agent_object_get_socket				(AgentObject* self);


DeviceInformation*								agent_object_get_information		(AgentObject* self);






G_END_DECLS
#endif // !__AGENT_OBJECT__