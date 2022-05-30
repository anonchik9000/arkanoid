#ifndef B2_W_CALLBACKS_H
#define B2_W_CALLBACKS_H

#include <structs.h>

typedef void (__stdcall* CallbackDebug)(char*);

typedef void (__stdcall *ListOfCollisionDataCallback)(int32, CollisionCallbackData[]);
typedef void (__stdcall *CollisionCallback)(CollisionCallbackData);
typedef void (__stdcall *DrawDbgCircleCallback)(Vector2, float, Box2dColor);
typedef void (__stdcall *DrawDbgSegmentCallback)(Vector2, Vector2, Box2dColor);
typedef void (__stdcall *DrawDbgTransformCallback)(Vector2, Vector2, Box2dColor);
typedef void (__stdcall *DrawDbgPolygonCallback)(Vector2[b2_maxPolygonVertices], int32, Box2dColor);
typedef void (__stdcall *ListOfPointersCallback)(int32, void* []);

#endif