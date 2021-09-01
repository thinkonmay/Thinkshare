#ifndef __HUMAN_INTERFACE_OPCODE_H__
#define __HUMAN_INTERFACE_OPCODE_H__


/*HID data channel opcode*/
typedef enum
{
	KEYUP,
	KEYDOWN,

	MOUSE_WHEEL,
	MOUSE_MOVE,
	MOUSE_UP,
	MOUSE_DOWN,

	KEYRESET,

}HidOpcode;


#endif