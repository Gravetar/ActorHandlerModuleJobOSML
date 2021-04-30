using System;

using NodaTime; // Отсюда LocalTime
using NetTopologySuite.Geometries;  // Отсюда Point и другая геометрия
using NetTopologySuite.Mathematics; // Отсюда векторы

using OSMLSGlobalLibrary.Modules;  // Отсюда OSMLSModule

using ActorModule;

using OSMLSGlobalLibrary.Map;

using PathsFindingCoreModule;

using ActorHandlerModuleJob;

namespace ActorHandlerTestModuleJob
{
    public class ActorHandlerTestModuleJob : OSMLSModule
    {
        [CustomStyle(@"new style.Style({
                stroke: new style.Stroke({
                    color: 'rgba(90, 0, 157, 1)',
                    width: 2
                })
            });
        ")]
        private class Highlighted : GeometryCollection
        {
            public Highlighted(Geometry[] geometries) : base(geometries)
            {

            }
        }

        // Координаты центра Москвы. Сюда будем закидывать акторов
        private double x = 4173165;
        private double y = 7510997;

        // Зададим радиус, в котором будут ходить акторы
        private double radius = 100;

        //Генератор случайных чисел
        private Random random = new Random();

        // И случайное смещение от центра, которое будем использовать для создания точек интереса
        private double offset { get { return random.NextDouble() * 2 * radius - radius; } }

        // Этот метод будет вызван один раз при запуске, соответственно тут вся инициализация
        protected override void Initialize()
        {
            // Создаем состояние шаблон. Потом каждому человеку зададим свои точки интересов
            State state = new State()
            {
                Hunger = 100,

                // Интервал можно задать через объекты LocalTime
                HomeTime = new TimeInterval(new LocalTime(20, 0), new LocalTime(22, 41)),

                // А можно через часы и минуты (и секунды тоже, если есть такая необходимость)
                JobTime = new TimeInterval(new LocalTime(20, 0), new LocalTime(18, 26))


            };

            // Создаём акторов
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Creating actor {i + 1}");

                // Делаем для каждого точку дома и точку работы в квадрате заданного радиуса от точки спавна
                state.Home = new Point(x + offset, y + offset);
                state.Job = new Point(x + offset, y + offset);

                Console.WriteLine($"Home at {state.Home.X}, {state.Home.Y}; " +
                                  $"Job at {state.Job.X}, {state.Job.Y}");

                // Создаём актора с заданным состоянием
                // Так как в конструкторе актора состояние копируется, 
                // можно использовать один и тот же объект состояния для инициализации,
                // при этом каждый актор получит отдельный объект, не связанный с другими
                Actor actor = new Actor(x, y, state);

                // Добавляем актора в объекты карты
                MapObjects.Add(actor);

                var firstCoordinate = new Coordinate(x, y);
                var secondCoordinate = new Coordinate(state.Job.X, state.Job.Y);

                Console.WriteLine($"Coor {firstCoordinate} and {secondCoordinate}");

                Console.WriteLine("Building path...");
                MapObjects.Add(new Highlighted(new Geometry[]
                    {
                                PathsFinding.GetPath(firstCoordinate, secondCoordinate, "Walking").Result
                    }));
                Console.WriteLine("Path was builded");
            }

            // Получаем список акторов на карте и выводим их количество
            var actors = MapObjects.GetAll<Actor>();
            Console.WriteLine($"Added {actors.Count} actors");

            foreach (var actor in actors)
                Console.WriteLine($"Actor on ({actor.Coordinate.X}, {actor.Coordinate.Y})\n" +
                                  $"\tHome at {actor.State.Home.X}, {actor.State.Home.Y}\n" +
                                  $"\tJob at {actor.State.Job.X}, {actor.State.Job.Y}");
        }
        // Этот метод вызывается регулярно, поэтому тут все действия, которые будут повторяться
        public override void Update(long elapsedMilliseconds)
        {
            //Console.WriteLine("\nActorTestModule: Update");

            // Снова получаем список акторов
            var actors = MapObjects.GetAll<Actor>();
            // Console.WriteLine($"Got {actors.Count} actors\n");

            // Для каждого актёра проверяем условия и назначаем новую активность если нужно
            foreach (var actor in actors)
            {
                // Дальше идёт куча првоверок
                // Основная их цель - убедиться, что я не перезаписываю одну и ту же активность на каждой итерации
                // Это может быть полезно, если, например, создание активности затрачивает много времени

                // Не уверен, что это правильный способ решать данную проблему, но другого способа я не знаю

                // Есть ли активность
                bool isActivity = actor.Activity != null;

                // Если активность есть, то наша ли это активность
                bool goActivity = isActivity ? actor.Activity is MovementActivityJob : false;

                // Если активность наша, ведёт ли она актора домой или на работу
                bool goWork = goActivity ? (actor.Activity as MovementActivityJob).Destination == actor.State.Job : false;


                Console.WriteLine($"Flags: {isActivity} {goActivity} {goWork}");

                //if (actor.State.JobTime.Ongoing) // Если сейчас вермя идти на работу
                //{
                    // Если активности нету, или активность не наша, или активность наша, но не ведет на работу
                    if (!isActivity)
                    {
                        // Назначить актору путь до работы
                        actor.Activity = new MovementActivityJob(actor.State.Job);
                        Console.WriteLine("Said actor go work\n");
                    }
                //}

                // Здесь я не проверяю приоритет (не знаю, по каким правилам его стоит задавать), 
                // но его следует проверять до задания активности. Например:
                //
                //     int newPrioriy = ...;  // Считаем приоритет своей активности
                //
                //     if (newPriority > actor.Activity.Priority)  // Задаём активность актору, если приоритет выше текущей
                //         actor.Activity = new GoActivity(actor.State.Job, newPriority);
            }
        }
    }
}
