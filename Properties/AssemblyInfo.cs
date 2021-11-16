using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Общие сведения об этой сборке предоставляются следующим набором
// набор атрибутов. Измените значения этих атрибутов, чтобы изменить сведения,
// связанные со сборкой.
[assembly: AssemblyTitle("MozaikaApp")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("FreezeMyTime")]
[assembly: AssemblyProduct("MozaikaApp")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// из модели COM, установите атрибут ComVisible для этого типа в значение true.
[assembly: ComVisible(false)]

//Чтобы начать создание локализуемых приложений, задайте
//<UICulture>CultureYouAreCodingWith</UICulture> в файле .csproj
//в <PropertyGroup>. Например, при использовании английского (США)
//в своих исходных файлах установите <UICulture> в en-US.  Затем отмените преобразование в комментарий
//атрибута NeutralResourceLanguage ниже.  Обновите "en-US" в
//строка внизу для обеспечения соответствия настройки UICulture в файле проекта.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //где расположены словари ресурсов по конкретным тематикам
                                     //(используется, если ресурс не найден на странице,
                                     // или в словарях ресурсов приложения)
    ResourceDictionaryLocation.SourceAssembly //где расположен словарь универсальных ресурсов
                                              //(используется, если ресурс не найден на странице,
                                              // в приложении или в каких-либо словарях ресурсов для конкретной темы)
)]


// Сведения о версии для сборки включают четыре следующих значения:
//
//      Основной номер версии
//      Дополнительный номер версии
//      Номер сборки
//      Номер редакции
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
//[assembly: AssemblyVersion("1.0.3")]
[assembly: AssemblyVersion("1.2.2.257")]
//[assembly: AssemblyFileVersion("1.0.3.0")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo("MozaikaApp.Tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010005f3936c0af85ebb2646e9f2f090922a956533a4010424466e5c52ce953d87bede74aa1826fa10b100c82878b3276076a0e14a66d8102bd866998e33195ebb06b7240fdff35124e20682fc3ef85352b54ee493edb9a9b6c3743342f4b215aa24d992fa6b65ee4d594a2d426e07838fd5b6a04989f9632bcabe320e245c2ec5ca")]
[assembly: InternalsVisibleTo("MozaikaApp.Explorables, PublicKey=002400000480000094000000060200000024000052534131000400000100010005f3936c0af85ebb2646e9f2f090922a956533a4010424466e5c52ce953d87bede74aa1826fa10b100c82878b3276076a0e14a66d8102bd866998e33195ebb06b7240fdff35124e20682fc3ef85352b54ee493edb9a9b6c3743342f4b215aa24d992fa6b65ee4d594a2d426e07838fd5b6a04989f9632bcabe320e245c2ec5ca")]
[assembly: AssemblyFileVersion("1.2.2.257")]
