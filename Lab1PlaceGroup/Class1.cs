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
namespace Lab1PlaceGroup
    //Отедльно подключены ссылки на RevitAPI и RevitAPIUI. В параметрах этих ссылок пункт "Копировать локально" = false
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GroupPickFilter : ISelectionFilter     //Создание класса для определения элементов, которые могут быть выбраны
    {
        public bool AllowElement(Element e)
        {
            return e.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSModelGroups);      //Сравнивает категорию данного элемента с категорией "группа моделей".
            /*
             * elem.Category.Id.IntegerValue - получает целочисленное значение идентификатора категории из элемента elem, переданного в качестве параметра в AllowElement().
             * BuiltInCategory.OST_IOSModelGroups - ссылается на число, идентифицирующее встроенную категорию "группы моделей", которое мы получаем из коллекции BuiltInCategory.
             * Поскольку члены перечисления на самом деле являются числами,произведено приведение для преобразования BuildingCategory.OST_IOSModelGroups в целое число, 
             * чтобы иметь возможность сравнить его со значением ID категории. 
             * 
            */
        }
        public bool AllowReference(Reference r, XYZ p)      //Если курсор наведен на ссылку, эта ссылка будет передана в метод AllowReference(), но ссылки в данной работе не нужны
        {
            return false;
        }
    }
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        { 
            //Получить объекты приложения и документа
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            try
            {
                //Определить ссылочный объект для принятия выбранного результата 
                Reference pickedRef = null;

                //Выбор группы
                Selection sel = uiapp.ActiveUIDocument.Selection;
                GroupPickFilter selFilter = new GroupPickFilter();
                pickedRef = sel.PickObject(ObjectType.Element, selFilter, "Пожалуйста выберите группу");        //Перегрузка для выбора объекта (1 - тип объекта, 2- фильтр выброа
                                                                                                                //экземпляр класса ISelectionFilter, стока с сообщением пользователю)
                Element elem = doc.GetElement(pickedRef);
                Group group = elem as Group;

                //Выбор точки
                XYZ point = sel.PickPoint("Пожалуйста выберите точку для размещения группы");

                //Размещение группы
                Transaction trans = new Transaction(doc);
                trans.Start("Lab");
                doc.Create.PlaceGroup(point, group.GroupType);
                trans.Commit();
            }

            //Если пользователь щелкнул правой кнопкой мыши или нажал Esc или нажать ПКМ во время выбора, обработать исключение
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)  
            {
                return Result.Cancelled;
            }
            catch (Exception ex)        //Обработка любого .NET исключения, например, из-за нехватки оперативки
            {
                message = ex.Message;
                return Result.Failed;
            }
           


            return Result.Succeeded;
        }
    }
    
}