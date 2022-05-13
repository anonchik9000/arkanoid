#include <stddef.h>
#include "box2d/headers/box2d.h"

#ifdef _WIN32
    #define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
    #define DllExport __attribute__((visibility("default")))
#endif

struct B2FilterReturnType
{
    public : uint16 categoryBits;
    public : uint16 maskBits;
    public : int16 groupIndex;
};

struct Vector2
{
    public : float x;
    public : float y;
};

struct BodyInfo
{
    public : Vector2 position;
    public : Vector2 linearVelocity;
    public : float angularVelocity;
    public : float angle;
};

struct CollisionCallbackData
{
    public : b2World* physicsWorld;
    public : int entityA;
    public : int entityB;

    public : Vector2 velA;
    public : Vector2 velB;

    public : Vector2 contactPoints[b2_maxManifoldPoints];
    public : int32 contactPointCount;

    public : Vector2 normal;
};

typedef void (__stdcall * Callback)(CollisionCallbackData);

class MyContactListener : public b2ContactListener
{
    private:

    b2World* m_world = NULL;

    void GetEntityContacts(CollisionCallbackData* data, b2Contact* contact)
    {
        data->entityA = (int)contact->GetFixtureA()->GetBody()->GetUserData().pointer;
        data->entityB = (int)contact->GetFixtureB()->GetBody()->GetUserData().pointer;

        b2WorldManifold worldManifold;
        contact -> GetWorldManifold(&worldManifold);

        GetContactVelocity(data, &worldManifold, contact);

        GetNormal(data, &worldManifold);

        GetContactPoints(data, &worldManifold);
    }

    void GetContactVelocity(CollisionCallbackData* data, b2WorldManifold* worldManifold, b2Contact* contact)
    {
        b2Vec2 v1 = contact->GetFixtureA()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold->points[0]);
        
        Vector2 vec1 {
            v1.x,
            v1.y
        };

        data->velA = vec1;

        b2Vec2 v2 = contact->GetFixtureB()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold->points[0]);
        
        Vector2 vec2 {
            v2.x,
            v2.y
        };

        data->velB = vec2;
    }

    void GetContactPoints(CollisionCallbackData* data, b2WorldManifold* worldManifold)
    {
        size_t length = (sizeof(worldManifold->points)/sizeof(*worldManifold->points));
        memcpy(data->contactPoints, worldManifold->points, length * sizeof(Vector2));
    }

    void GetNormal(CollisionCallbackData* data, b2WorldManifold* worldManifold)
    {
        Vector2 vec2 {
            worldManifold->normal.x,
            worldManifold->normal.y
        };
        data->normal = vec2;
    }

    void BeginContact(b2Contact* contact)
    {
        if (m_callbackBeginContact == NULL) return;

        CollisionCallbackData data;
        GetEntityContacts(&data, contact);

        m_callbackBeginContact(data);
    }

    void EndContact(b2Contact* contact)
    {
        if (m_callbackEndContact == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackEndContact(data);
    }

    void PreSolve(b2Contact* contact, const b2Manifold* oldManifold)
    {
        if (m_callbackPreSolve == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackPreSolve(data);
    }

    void PostSolve(b2Contact* contact, const b2ContactImpulse* impulse)
    {
        if (m_callbackPostSolve == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackPostSolve(data);
    }

    public:

    MyContactListener(b2World* world) : m_world(world) {};

    Callback m_callbackBeginContact = NULL;
    Callback m_callbackEndContact = NULL;
    Callback m_callbackPreSolve = NULL;
    Callback m_callbackPostSolve = NULL;
};

extern "C"
{
    DllExport void SetBeginContactCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackBeginContact = callback;
    }

    DllExport void SetEndContactCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackEndContact = callback;
    }

    DllExport void SetPreSolveCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPreSolve = callback;
    }

    DllExport void SetPostSolveCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPostSolve = callback;
    }


    DllExport b2World* UpdateWorld(b2World* world, float timeStep, int velocityIterations, int positionIterations)
    {
        world -> Step(timeStep, velocityIterations, positionIterations);

        return world;
    }

    DllExport b2World* CreateWorld(Vector2 gravity)
    {
        b2Vec2 bGravity(gravity.x, gravity.y);
        b2World* world = new b2World(bGravity);

        MyContactListener* myContactListener = new MyContactListener(world);

        world -> SetContactListener(myContactListener);

        return world;
    }

    DllExport void DestroyWorld(b2World* world)
    {
        delete world -> GetContactListener();
        delete world;
        world = nullptr;
    }

    DllExport b2Body* CreateBody(b2World* world, int bodyType, Vector2 position, float angle, int entity)
    {
        b2BodyDef bodyDef;
        bodyDef.type = (b2BodyType)bodyType;
        bodyDef.position.Set(position.x, position.y);
        bodyDef.angle = angle;
        bodyDef.userData.pointer = entity;

        b2Body* body = world -> CreateBody(&bodyDef);
        body -> SetFixedRotation(false);

        return body;
    }

    DllExport b2Shape* CreateChainShape(Vector2 vertices[], int count)
    {
        b2Vec2* b2Vertices = (b2Vec2*)b2Alloc(count * sizeof(b2Vec2));
        memcpy(b2Vertices, vertices, count * sizeof(b2Vec2));

        b2ChainShape* shape = new b2ChainShape();
        shape->CreateLoop(b2Vertices, count);

        return shape;
    }

    DllExport b2Shape* CreatePolygonShape(Vector2 vertices[], int count)
    {
        b2Vec2* b2Vertices = (b2Vec2*)b2Alloc(count * sizeof(b2Vec2));
        memcpy(b2Vertices, vertices, count * sizeof(b2Vec2));

        b2PolygonShape* shape = new b2PolygonShape();
        shape->Set(b2Vertices, count);
        return shape;
    }

    DllExport b2Shape* CreateCircleShape(float radius)
    {
        b2CircleShape* shape = new b2CircleShape();
        shape->m_radius = radius;
        return shape;
    }

    DllExport b2Shape* CreateBoxShape(Vector2 extents)
    {
        b2PolygonShape* shape = new b2PolygonShape();
        shape->SetAsBox(extents.x, extents.y);
        return shape;
    }

    DllExport void AddFixtureToBody(b2Body* body, b2Shape* shape, float density, float friction, float restitution,
        float restitutionThreshold, bool isTrigger, b2Filter filter)
    {
        b2FixtureDef fixtureDef;
        fixtureDef.shape = shape;
        fixtureDef.density = density;
        fixtureDef.friction = friction;
        fixtureDef.restitution = restitution;
        fixtureDef.restitutionThreshold = restitutionThreshold;
        fixtureDef.isSensor = isTrigger;
        fixtureDef.filter = filter;

        body -> CreateFixture(&fixtureDef);
        b2Free(shape);
    }

    DllExport float GetLinearDamping(b2Body* body)
    {
        return body -> GetLinearDamping();
    }

    DllExport float GetAngularDamping(b2Body* body)
    {
        return body -> GetAngularDamping();
    }

    DllExport void SetLinearDamping(b2Body* body, float val)
    {
        body -> SetLinearDamping(val);
    }

    DllExport void SetAngularDamping(b2Body* body, float val)
    {
        body -> SetAngularDamping(val);
    }

    DllExport void DestroyBody(b2World* world, b2Body* body)
    {
        world -> DestroyBody(body);
    }

    DllExport Vector2 GetPosition(b2Body* body)
    {
        const b2Vec2& position = body -> GetPosition();

        Vector2 sharedPos {
            position.x,
            position.y
        };

        return sharedPos;
    }

    DllExport void SetPosition(b2Body* body, Vector2 position)
    {
        float angle = body -> GetAngle();
        b2Vec2 bPosition(position.x, position.y);

        body -> SetTransform(bPosition, angle);
        body -> SetAwake(true);
    }

    DllExport float GetAngle(b2Body* body)
    {
        return body -> GetAngle();
    }

    DllExport void SetAngle(b2Body* body, float angle)
    {
        b2Vec2 position = body -> GetPosition();

        body -> SetTransform(position, angle);
    }

    DllExport void SetLinearVelocity(b2Body* body, Vector2 linearVelocity)
    {
        b2Vec2 bLinearVelocity(linearVelocity.x, linearVelocity.y);

        body -> SetLinearVelocity(bLinearVelocity);
    }

    DllExport void SetAngularVelocity(b2Body* body, float angularVelocity)
    {
        body -> SetAngularVelocity(angularVelocity);
    }

    DllExport Vector2 GetLinearVelocity(b2Body* body)
    {
        const b2Vec2 linearVelocity =  body -> GetLinearVelocity();

        struct Vector2 vec {
            linearVelocity.x,
            linearVelocity.y
        };

        return vec;
    }

    DllExport float GetAngularVelocity(b2Body* body)
    {
        return  body -> GetAngularVelocity();
    }

    DllExport void ApplyForce(b2Body* body, Vector2 force, Vector2 point)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyForce(bForce, bPoint, true);
    }

    DllExport void ApplyForceToCenter(b2Body* body, Vector2 force)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyForceToCenter(bForce, true);
    }

    DllExport void ApplyLinearImpulse(b2Body* body, Vector2 force, Vector2 point)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyLinearImpulse(bForce, bPoint, true);
    }

    DllExport void ApplyLinearImpulseToCenter(b2Body* body, Vector2 force)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyLinearImpulseToCenter(bForce, true);
    }

    DllExport void SetEnabled(b2Body* body, bool flag)
    {
        body -> SetEnabled(flag);
    }

    DllExport bool IsEnabled(b2Body* body)
    {
        return body -> IsEnabled();
    }

    DllExport BodyInfo GetBodyInfo(b2Body* body)
    {
        BodyInfo bodyInfo;
        bodyInfo.position = GetPosition(body);
        bodyInfo.linearVelocity = GetLinearVelocity(body);
        bodyInfo.angularVelocity = GetAngularVelocity(body);
        bodyInfo.angle = GetAngle(body);

        return bodyInfo;
    }

    DllExport void SetBodyInfo(b2Body* body, BodyInfo bodyInfo)
    {
        SetAngle(body, bodyInfo.angle);
        SetPosition(body, bodyInfo.position);
        SetLinearVelocity(body, bodyInfo.linearVelocity);
        SetAngularVelocity(body, bodyInfo.angularVelocity);
    }

    DllExport void SetBullet(b2Body* body, bool flag)
    {
        body -> SetBullet(flag);
    }

    DllExport B2FilterReturnType GetBodyFixturesFilterData(b2Body* body)
    {
        b2Fixture fixture = body -> GetFixtureList()[0];
        b2Filter filter = fixture.GetFilterData();
        B2FilterReturnType filterResult
        {
            filterResult.categoryBits = filter.categoryBits,
            filterResult.maskBits = filter.maskBits,
            filterResult.groupIndex = filter.groupIndex
        };
        
        return filterResult;
    }

    DllExport bool RayCast(b2World* world, Vector2 origin, Vector2 direction, float distance)
    {
        b2Vec2 position(origin.x, origin.y);
        b2RayCastOutput output;
        
        b2RayCastInput ray;
        ray.p1.Set(position.x, position.y);
        ray.p2.Set(position.x + direction.x * distance, position.y + direction.y * distance);
        ray.maxFraction = distance;

        for (b2Body* body = world -> GetBodyList(); body; body = body -> GetNext())
        {
            b2Fixture fixture = body -> GetFixtureList()[0];
            bool hit = fixture.RayCast(&output, ray, 0);

            if(hit)
            {
                return true;
            } 
        }

        return false;
    }
}