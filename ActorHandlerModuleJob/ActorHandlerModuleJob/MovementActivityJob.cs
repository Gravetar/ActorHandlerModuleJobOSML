using System;

using NodaTime; // Отсюда LocalTime
using NetTopologySuite.Geometries;  // Отсюда Point и другая геометрия
using NetTopologySuite.Mathematics; // Отсюда векторы

using OSMLSGlobalLibrary.Modules;  // Отсюда OSMLSModule

using ActorModule;

using OSMLSGlobalLibrary.Map;

using PathsFindingCoreModule;

namespace ActorHandlerModuleJob
{
    public class MovementActivityJob : IActivity
    {
        public Coordinate[] Path;
        public int i = 0;

        public bool IsPath = true;

        // Приоритет делаем авто-свойством, со значением по умолчанию
        // Вообще он дожен был быть полем, но интерфейсы не дают объявлять поля, так что...
        public int Priority { get; set; } = 1;

        // Точка назначения
        public Point Destination { get; }

        public MovementActivityJob(Point destination)
        {
            Destination = destination;
        }

        public MovementActivityJob(Point destination, int priority)
        {
            Destination = destination;
            Priority = priority;
        }

        // Здесь происходит работа с актором
        public bool Update(Actor actor, double deltaTime)
        {
            double speed = 10;  //Скорость в м/с. По хорошему её в State определить

            // Расстояние, которое может пройти актор с заданной скоростью за прошедшее время
            double distance = speed * deltaTime;

            if (IsPath)
            {
                var firstCoordinate = new Coordinate(actor.X, actor.Y);
                var secondCoordinate = new Coordinate(Destination.X, Destination.Y);

                Path = PathsFinding.GetPath(firstCoordinate, secondCoordinate, "Walking").Result.Coordinates;
                IsPath = false;
            }

            Vector2D direction = new Vector2D(actor.Coordinate, Path[i]);
            // Проверка на перешагивание

            if (direction.Length() <= distance)
            {
                // Шагаем в точку, если она ближе, чем расстояние которое можно пройти
                actor.X = Path[i].X;
                actor.Y = Path[i].Y;
            }
            else
            {
                // Вычисляем новый вектор, с направлением к точке назначения и длинной в distance
                direction = direction.Normalize().Multiply(distance);

                // Смещаемся по вектору
                actor.X += direction.X;
                actor.Y += direction.Y;
            }

            if (actor.X == Path[i].X && actor.Y == Path[i].Y && i < Path.Length - 1)
            {
                i++;

                Console.WriteLine(i);
                Console.WriteLine(Path.Length);
            }

            // Если в процессе шагания мы достигли точки назначения
            if (actor.X == Path[Path.Length - 1].X && actor.Y == Path[Path.Length - 1].Y)
            {
                Console.WriteLine("Start Waiting");
                actor.Activity = new WaitingActivityJob(actor.State.HomeTime);
                i = 0;
                IsPath = true;
            }
            return false;
        }
    }
}
