using System.Collections.Generic;
namespace LibAurora.Physics;

public interface ISpaceQuery
{
	void AddShape(CollisionShape shape);
	void RemoveShape(CollisionShape shape);
	void Clear();
	IEnumerable<CollisionShape> GetCollisionShapes(CollisionShape shape);
	void Update(double delta);
	void DebugDraw();
	
	void Dirt(CollisionShape? shape);
}