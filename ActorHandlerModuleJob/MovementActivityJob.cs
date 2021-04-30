using System;
using NetTopologySuite.Geometries;  // Отсюда Point и другая геометрия
using NetTopologySuite.Mathematics; // Отсюда векторы
using ActorModule;
using PathsFindingCoreModule;

namespace ActorHandlerModuleJob
{
    public class MovementActivityJob : IActivity
    {
        //Координаты пути до работы и счетчик пути
        public Coordinate[] Path;
        public int i = 0;
        //Флаг-путь построен.
        public bool IsPath = false;

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

            //Если путь еще не построен
            if (!IsPath)
            {
                //Начальные координаты и координаты точки работы
                var firstCoordinate = new Coordinate(actor.X, actor.Y);
                var secondCoordinate = new Coordinate(Destination.X, Destination.Y);
                //Строим путь
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
            //Если актор достиг следующей точки пути
            if (actor.X == Path[i].X && actor.Y == Path[i].Y && i < Path.Length - 1)
            {
                i++;
                //Console.WriteLine(i);
                //Console.WriteLine(Path.Length);
            }

            // Если в процессе шагания мы достигли точки назначения
            if (actor.X == Path[Path.Length - 1].X && actor.Y == Path[Path.Length - 1].Y)
            {
                Console.WriteLine("Start Waiting");
                //Запуск активити ожидания(имитация нахождения на работе)
                actor.Activity = new WaitingActivityJob(actor.State.HomeTime);
                i = 0;
                IsPath = false;
            }
            return false;
        }
    }
}
