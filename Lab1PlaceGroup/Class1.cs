using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;

//Подключить через ссылки RevitAPI и RevitAPIUI, в свосйствах указать "Копировать локально" = false

namespace Lab1PlaceGroup
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) 
        {
            //Получить объекты приложений и документов
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {
                //Определите ссылочный объект, который будет принимать результат пикировки
                Reference pickedRef = null;

                //Выберите группу
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Please select a group");     // Перегрузка для выбора объекта(1 - тип объекта, 2 - фильтр выброа
                                                                                                        //экземпляр класса ISelectionFilter, стока с сообщением пользователю)
                Element elem = doc.GetElement(pickedRef);
                Group group = elem as Group;

                // Получение центральной точки группы
                XYZ origin = GetElementCenter(group);

                // Получить помещение, в котором находится выбранная группа
                Room room = GetRoomOfGroup(doc, origin);

                // Получение центральной точки комнаты
                XYZ sourceCenter = GetRoomCenter(room);
                string coords = "X = " + sourceCenter.X.ToString() + "\r\n" + "Y = " + sourceCenter.Y.ToString() + "\r\n" + "Z = " + sourceCenter.Z.ToString();     //Преобразование
                                                                                                                //значений в строку для показа в диалоговом окне
                TaskDialog.Show("Source room Center", coords);      //Отображение диалогового окна, первое - заголовок, второе - параметр

                //Выбор точки
                //   XYZ point = sel.PickPoint("Пожалуйста выберите группу");

                // Разместить группу
                Transaction trans = new Transaction(doc);
                trans.Start("Lab");
                // doc.Create.PlaceGroup(point, group.GroupType);

                // Вычислить позицию новой группы
                XYZ groupLocation = sourceCenter + new XYZ(20, 0, 0);
                doc.Create.PlaceGroup(groupLocation, group.GroupType);
                trans.Commit();

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Result.Succeeded;
        }
        public class GroupPickFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                return (e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups)); //Сравнивает категорию данного элемента с категорией "группа моделей".
                /*
                 * elem.Category.Id.IntegerValue - получает целочисленное значение идентификатора категории из элемента elem, переданного в качестве параметра в AllowElement().
                 * BuiltInCategory.OST_IOSModelGroups - ссылается на число, идентифицирующее встроенную категорию "группы моделей", которое мы получаем из коллекции BuiltInCategory.
                 * Поскольку члены перечисления на самом деле являются числами,произведено приведение для преобразования BuildingCategory.OST_IOSModelGroups в целое число, 
                 * чтобы иметь возможность сравнить его со значением ID категории. 
                 */
            }
            public bool AllowReference(Reference r, XYZ p)
            {
                return false;
            }

        }
        public XYZ GetElementCenter(Element elem)
        {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);       //Доступ к свойству BoundingBox переданного элемента, сохранив его значение в переменной с именем bounding
            XYZ center = (bounding.Max + bounding.Min) * 0.5;       //Расчёт среднего значения путём сложения макс точек геомктрии и делении на 2
            return center;
        }
        Room GetRoomOfGroup(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);     //Объект коллектор фильтрует элементы в документе
            collector.OfCategory(BuiltInCategory.OST_Rooms);        //Фильтр категории только по комнатам
            Room room = null;
            foreach (Element elem in collector)     //Перебор комнат
            {
                room = elem as Room;        //Если элемент не Room, то room = null
                if (room != null)
                {
                    // Определите, находится ли эта точка в выбранной комнате
                    if (room.IsPointInRoom(point))
                    {
                        break;
                    }
                }
            }
            return room;
        }
        /// Возвращает координаты центральной точки комнаты.
        /// Значение Z равно нижней части комнаты
        public XYZ GetRoomCenter(Room room)
        {
            // Get the room center point.
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;     //каст room к LocalPoint
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);      //Возвращение модифицированной точки
            return roomCenter;
        }
    }
    // Фильтр для ограничения выборки по комнатам
    public class RoomPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return
(e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Rooms));
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }

}