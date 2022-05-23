using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1519
//ReSharper disable UnusedMember.Global
//ReSharper disable UnusedParameter.Local
//ReSharper disable MemberCanBePrivate.Global
//Resharper disable UnusedAutoPropertyAccessor.Global
//ReSharper disable IntroduceOptionalParameters.Global
//ReSharper disable MemberCanProtected.Global
//ReSharper disable InconsistentNaming

namespace VOS.Annotations
/************************************************************
 * Purpose: Indicates that the value of the marked elements
 *          could be <c>NULL</c> sometimes, So the check for 
 *          <c>NULL</c> is necessary before its usage
 * PseudoCode: 
 * [CanBeNull] public object Test() { return null;}
 * public void UseTest(){
 *          var p = Test();
 *          var s = p.ToString();
 *          //Warning: possible NullReferenceException
 * }
 ************************************************************/
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property
        | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]

    public sealed class CanBeNullAttribute : Attribute { }
    /************************************************************
     * Purpose: Indicates that the value of the marked element
     *          could never be null
     * 
     * PseudoCode: 
     * [NotNull] public object Foo()[
     *         return null; 
     *         //Warning: possible 'null' assignment
     * }
    ************************************************************/
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
        AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotNullAttribute : Attribute
    { }
    /************************************************************
     * Purpose: Indicates that the marked method builds strings by
     *          format Patterns and optional arguments. Parameter, 
     *          which contains format string, should be given in 
     *          constructor. The format string should be in 
     *          cref="string.Format(IFormatProvider, string,object[])
     * PseudoCode:
     * [StringFormatMethod("message")]
     *  public void showError( string message, params object[] args){
     *          // do something 
     * }
     * public void Foo(){
     *          ShowError("Failed: {0}"); 
     *          //Warning: Non-existing argument in format string
     *  }
     ************************************************************/
    [AttributeUsage(AttributeTargets.Constructor |
            AttributeTargets.Method,
            AllowMultiple = false,
            Inherited = true)]

    public sealed class StringFormatMethodAttribute : Attribute
    {
        /************************************************************
        * Purpose:  Specifies which parameter of an annotated method 
        * should be treated as format-string
        * PseudoCode: 
        * <param name = "formatParameterName"></param>
        ************************************************************/
        public StringFormatMethodAttribute(string formateParameterName)
        {
            FormatParameterName = formateParameterName;
        }
        public string FormatParameterName
        {
            get;
            private set;
        }
    }

    /************************************************************
    * Purpose: Indicates that the function argument should be string
    * literal and match one of the parameters of the caller function
    * For example ReSharper annotates the parameter of 
    * System.ArgumentNullException
    * PseudoCode: public void Foo(string param){
    *          if (param == null)
    *              throw new ArgumentNullException("par");
    *              //Warning: cannot resolve symbol
    *          }
    ************************************************************/
    [AttributeUsage(AttributeTargets.Parameter,
        AllowMultiple = false, Inherited = true)]

    public sealed class InvokerParameterNameAttribute : Attribute
    {
        /************************************************************
        * Purpose: Indicates that the method is contained in a type 
        * that implements System.ComponentModel.INotifyPropertyChanged
        * The method should be non-static and confomr to one of the
        * supported signatures
        * <remarks><list>
        *   <item><c>NotifyChanged(string)</c></item>
        *   <item><c>NotifyChanged(params string[])</c></item>
        *   <item><c>NotifyChanged{T}(Expression{Func{T}})</c></item>
        *   <item><c>NotifyChanged{T,U}(Expression{Func{T,U}})</c></item>
        *   <item><c>SetProperty{T}(ref T, T, string)</c></item>
        * </remarks></list>
        * PseudoCode: public class Foo : INotifyPropertyChanged {
        *               public event PropertyChangedEventHandler PropertyChanged;
        *               [NotifyPropertyChangedInvocator]
        *               protected virtual void NotifyChanged(string propertyName) { ... }
        *               private string _name;
        *               public string Name { get { return _name; }
        *               set { _name = value; NotifyChanged("LastName"); //Warning
        *               }
        *       }
        * }
        * Examples of generated notifications:
        * <list>
        *   <item><c>NotifyChanged("Property")</c></item>
        *   <item><c>NotifyChanged(() =&gt; Property)</c></item>
        *   <item><c>NotifyChanged((VM x) =&gt; x.Property)</c></item>
        *   <item><c>SetProperty(ref myField, value, "Property")</c></item>
        * </list>
        ************************************************************/
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]

    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {
        public NotifyPropertyChangedInvocatorAttribute()
        {

        }
        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
        public string ParameterName
        {
            get;
            private set;
        }
    }

    /*********************************************************************************************
    * Purpose: Describes dependency between methods input & output
    * If method has single input parameter, it's name could be omitted 
    * for method output means that the methos doesn't return normally
    * canbenull annotation is only applicable for output parameters.
    * You can use multiple[ContractAnnotation] for each FDT row,
    * or use single attribute with rows separated by semicolon.
    * PseudoCode:
    *   Function Definition Table syntax:
    *           FDT      ::= FDTRow [;FDTRow]*
    *           FDTRow   ::= Input =&gt; Output | Output &lt;= Input
    *           Input    ::= ParameterName: Value [, Input]*
    *           Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}
    *           Value    ::= true | false | null | notnull | canbenull
    *           [ContractAnnotation("=> halt")]
    *           public void TerminationMethod()
    *                   [ContractAnnotation("halt &lt;= condition: false")]
    *           public void Assert(bool condition, string text) // regular assertion method
    *                   [ContractAnnotation("s:null => true")]
    *           public bool IsNullOrEmpty(string s) // string.IsNullOrEmpty()
    *                   //A method that returns null if the parameter is null, 
    *                   and not null if the parameter is not null
    *                   [ContractAnnotation("null => null; notnull => notnull")]
    *           public object Transform(object data) 
    *                   [ContractAnnotation("s:null=>false; =>true,result:notnull; =>false, result:null")]
    *          public bool TryParse(string s, out Person result)
    *********************************************************************************************/
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]

    public sealed class ContractAnnotationAttribute : Attribute
    {
        public ContractAnnotationAttribute([NotNull] string contract)
        : this(contract, false) { }

        public ContractAnnotationAttribute([NotNull] string contract, bool forceFullStates)
        {
            Contract = contract;
            ForceFullStates = forceFullStates;
        }
        public string Contract
        {
            get;
            private set;
        }
        public bool ForceFullStates
        {
            get;
            private set;
        }
    }

    /*********************************************************************
     * Purpose: Indicates that marked element should be localized or not
     * Checks to see if the marked element should be localized
     * PseudoCode: 
     *      [LocalizationRequiredAttribute(true)]
     *      public class Foo {
     *              private string str = "my string"; 
     *              //Throws Warning: Localizzable string
     *       }
     *       
    *********************************************************************/
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]

    public sealed class LocalizationRequiredAttribute : Attribute
    {
        public LocalizationRequiredAttribute() :
            this(true)
        {

        }
        public LocalizationRequiredAttribute(bool requisition)
        {
            Requisition = requisition;
        }
        public bool Requisition
        {
            get;
            private set;
        }
    }

    /*****************************************************************
     * Purpose: Checks for value of the marked type or its derivatives
     * PseduoCode:
     * [CannotApplyEqualityOperator]
     * class NoEquality { }
     *      class UsesNoEquality { 
     *              public void Test() {
     *                  var ca1 = new NoEquality();
     *                  var ca2 = new NoEquality();
     *                  if (ca1 != null) { // OK   
     *                  bool condition = ca1 == ca2; // Throws Warning
    ******************************************************************/
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class
                    | AttributeTargets.Struct,
                    AllowMultiple = false, Inherited = true)]

    public sealed class CannotApplyEqualityOperatorAttribute : Attribute { }

    /*******************************************************************
     * Purpose: When applied to a target attribute, it specifies a 
     *          requirement for any type marked with the target attribute
     *          to implement/inherit specific data type/types
     * PseudoCode: [BaseTypeRequired(typeof(IComponent)]
     *              public class ComponentAttribute: Attribute {}
     *                      [Component]
     *              public class MyComponent: IComponent {}
     *******************************************************************/

    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = true, Inherited = true)]
    [BaseTypeRequired(typeof(Attribute))]

    public sealed class BaseTypeRequiredAttribute : Attribute
    {
        public BaseTypeRequiredAttribute([NotNull] Type baseType)
        {
            BaseType = baseType;
        }
        [NotNull]
        public Type BaseType
        {
            get; //gets values
            private set; //sets value
        }
    }

    /*******************************************************************
     * Purpose: Checks for marked symbol if its used implicitly 
     *          or explicitly. The symbol will not be marked as unused
     *          (usage inspections)
    *******************************************************************/
    [AttributeUsage(AttributeTargets.All,
                    AllowMultiple = false, Inherited = true)]

    public sealed class UsedImplicitlyAttribute : Attribute
    {
        public UsedImplicitlyAttribute() :
                this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
        { }

        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags) :
                this(useKindFlags, ImplicitUseTargetFlags.Default)
        { }

        public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags) :
               this(ImplicitUseKindFlags.Default, targetFlags)
        { }

        public UsedImplicitlyAttribute(
            ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        public ImplicitUseKindFlags UseKindFlags
        {
            get;
            private set;
        }
        public ImplicitUseTargetFlags TargetFlags
        {
            get;
            private set;
        }
    }

    /******************************************************************
     * Purpose: Used on attributes and causes ReSharper to not mark 
     *          symbols thathave been marked when such attributes 
     *          are unused
     *****************************************************************/
    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = false, Inherited = true)]

    public sealed class MeansImplicitUseAttribute : Attribute
    {
        public MeansImplicitUseAttribute() :
               this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
        { }

        public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags) :
                this(useKindFlags, ImplicitUseTargetFlags.Default)
        { }

        public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags) :
                this(ImplicitUseKindFlags.Default, targetFlags)
        { }

        public MeansImplicitUseAttribute(
            ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }
        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags
        {
            get;
            private set;
        }
        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags
        {
            get;
            private set;
        }

    }

    [Flags]

    public enum ImplicitUseKindFlags
    {
        Default = Access | Assign | InstantiatedWithFixedConstructorSignature,
        //Only entity marked with attribute considered used
        Access = 1,
        //Indicates implicit assignment to a member
        Assign = 2,
        //Indicates implicit instantiation of a type with fixed constructor signatures
        //Unused constructor parameters won't be reported as such
        InstantiatedWithFixedConstructorSignature = 4,
        //Indicates implicit instantiation of a data type
        InstantiatedNoFixedConstructorSignature = 8
    }
    /******************************************************************
    * Purpose: Specifies what is considered implicitly used when marked
    * PseudoCode: 
    *       ref = MeansImplicitUseAttribute 
    *       ref = UsedImplicitlyAttribute
    *****************************************************************/
    [Flags]

    public enum ImplicitUseTargetFlags
    {
        Default = Itself,
        Itself = 1,
        //Marked members with attribute considered as used
        Members = 2,
        //Marked attribute and all its considered usage
        WithMembers = Itself | Members
    }

    /******************************************************************
     * Purpose: Intended to mark publicly avaibale API's, which should 
     *          not be removed as its considered used 
     *****************************************************************/
    [MeansImplicitUse]

    public sealed class PublicAPIAttribute : Attribute
    {
        public PublicAPIAttribute() { }

        public PublicAPIAttribute([NotNull] string comment)
        {
            Comment = comment;
        }

        [NotNull]
        public string Comment
        {
            get;
            private set;
        }
    }

    /******************************************************************
     * Purpose: Transmits to code analysis engine if the parameter is 
     *          completely handled when the invoked method is on stack.
     *          If the paramter is a delegate, then that delegate is 
     *          executed while the method is executed. 
     *          If the parameter is an enumerable, indicates that it is 
     *          enumerated while method is executed
     *****************************************************************/
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]

    public sealed class InstantHandleAttribute : Attribute { }

    /******************************************************************
     * Purpose: Indicates that a method does not make any state changes
     *          and is not observable by the machine
     * PseudoCode:
     * [Pure] private int Multiply(int x, int y){
     *                  return x * y; }
     *        public void Foo() {
     *                  const int a = 2, b = 2;
     *                  Multiply(a,b);
     *        //Warning : Return Value of pure method is not used }
     *****************************************************************/
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class PureAttribute : Attribute { }

    /******************************************************************
     * Purpose: Indicates that a parameter is a path to a file within 
     *          the project files, Path can be relative or absolute
     ******************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PathReferenceAttribute : Attribute
    {
        public PathReferenceAttribute() { }

        public PathReferenceAttribute([PathReference] string basePath)
        {
            BasePath = basePath;
        }
        [NotNull]
        public string BasePath
        {
            get;
            private set;
        }
    }
    /******************************************************************
     * Purpose: ASP.NET ModelViewController MVC Architecture Attributes
     *****************************************************************/
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCAreaMasterLocationFormatAttribute : Attribute
    {
        public AspMVCAreaMasterLocationFormatAttribute(string format) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCAreaPartialViewLocationFormatAttribute : Attribute
    {
        public AspMVCAreaPartialViewLocationFormatAttribute(string format) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCAreaViewLocationFormatAttribute : Attribute
    {
        public AspMVCAreaViewLocationFormatAttribute(string format) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCMasterLocationFormatAttribute : Attribute
    {
        public AspMVCMasterLocationFormatAttribute(string format) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCPartialViewLocationFormatAttribute : Attribute
    {
        public AspMVCPartialViewLocationFormatAttribute(string format) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]

    public sealed class AspMVCViewLocationFormatAttribute : Attribute
    {
        public AspMVCViewLocationFormatAttribute(string format) { }
    }

    /******************************************************************
     * Purpose: ASP.NET ModelViewController MVC Architecture Attributes
     *          when applied to parameter, indicates that the parameter
     *          is an MVC action. action name is calculated implicitly
     *          from the context. Use this attribute for custom wrapper
     * PseudoCode: 
     *          System.Web.MVC.HTML.ChildActionExtension.RenderAction 
     *                  (HTMLHelper, String)
    ******************************************************************/

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]

    public sealed class AspMVCActionAttribute : Attribute
    {
        public AspMVCActionAttribute([NotNull] string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }
        [NotNull]
        public string AnonymousProperty
        {
            get;
            private set;
        }
    }

    /******************************************************************
    * Purpose: ASP.NET MVC attribute. Indicates that a parameter is an
    *           MVC area. Uses this attribute for custom wrappers
    * PseudoCode:
    *           System.Web.MVC.HTML.ChildActionExtension.RenderAction 
    *           (HTMLHelper, String)
    ******************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]

    public sealed class AspMVCAreaAttribute : PathReferenceAttribute
    {
        public AspMVCAreaAttribute([NotNull] string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }

        [NotNull]
        public string AnonymousProperty
        {
            get;
            private set;
        }
    }

    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. when applied to parameter,
    *           indicates that parameters in a controller action.
    *           If and when applied to a method, the controller name is calculated
    *           is calculated implicitly from the context. Use this attribute for
    *           custom wrappers similar to
    * 
    * PseudoCode:
    *           System.Web.MVC.HTML.ChildActionExtension.RenderAction
    *                                     (HTMLHelper, String, String)
    *****************************************************************************/

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]

    public sealed class AspMVCControllerAttribute : Attribute
    {
        public AspMVCControllerAttribute() { }

        public AspMVCControllerAttribute([NotNull] string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }
        [NotNull]
        public string AnonymousProperty
        {
            get;
            private set;
        }
    }
    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. Indicates that a parameter is an MVC master
    *          uses this attribute for custom wrappers
    * PseudoCode:
    *           System.Web.MVC.Controller.View(String, String)
    *****************************************************************************/

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMVCMasterAttribute : Attribute { }

    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. Indicates that a paramter is an MVC Model 
    *           type. 
    * PseudoCode:
    *           System.Web.MVC.Controller.View(String, Object)
    *****************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]

    public sealed class AspMVCModelTypeAttribute : Attribute { }

    /***************************************************************************************
    * Purpose: ASP.NET MVC attribute. if and when applied to a parameter, indicates
    *           that the parameter is a partial view. If applied to method, the 
    *           partial view name is calculated and assigned implicitly from the 
    *           content of a custom wrapper
    * PseudoCode: 
    *           System.Web.MVC.HTML.RenderPartialExtensions.RenderPartial(HTMLHelper, String)
    ****************************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]

    public sealed class AspMVCPartialViewAttribute : PathReferenceAttribute { }

    /***************************************************************************************
    * Purpose: Allows disabling of all inspections for mvc views within a class or method
    ****************************************************************************************/
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]

    public sealed class AspMVCSupressViewErrorAttribute : Attribute { }

    /****************************************************************************************
    * Purpose: ASP.NET MVC Attribute. Indicates that a parameter is in MVC view
    * PseudoCode:
    *           System.Web.MVC.HTML.DisplayExtension.DisplayForModel(HTMLHelper, String)
    ****************************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]

    public sealed class AspMVCDisplayTemplateAttribute : Attribute { }

    /********************************************************************************
    * Purpose: ASP.NET MVC Attribute. Indicates that a parameter is in MVC editor
    *           uses attribute for custom wrappers
    * PseudoCode:
    *           System.Web.MVC.HTML.EditorExtension.EditorForModel(HTMLHelper,String)
    * 
    ********************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]

    public sealed class AspMVCEditorTemplateAttribute : Attribute { }

    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. Indicates that a parameter is an MVC template
    * PseudoCode:
    *       System.ComponentModel.DataAnnotation.UIHintAttribute(System.String)
    *****************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter)]

    public sealed class AspMVCTemplateAttribute : Attribute { }

    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. If applied to a parameter, indicates that the
    *           parameter is an mvc view. If applied to amethod, the mvc view name
    *           is calculate implicitly from the context
    * PseudoCode:
    *           System.Web.MVC.Controller.View(Object)
    *****************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]

    public sealed class AspMVCViewAttribute : PathReferenceAttribute { }

    /*****************************************************************************
    * Purpose: ASP.NET MVC attribute. When applied to a parameter of an 
    *           attribute, indicates that this parameter is an MVC action name
    * PseudoCode: [ActionName("Foo")]
    *             public ActionResult Login(string returnURL){
    *                       ViewBag.ReturnUrl = Url.Action("Foo");
    *                       return RedirectToAction("Bar"); }
    *****************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class AspMVCActionSelectorAttribute : Attribute { }

    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field,
        Inherited = true)]

    public sealed class HTMLElementAttributesAttribute : Attribute
    {
        public HTMLElementAttributesAttribute() { }

        public HTMLElementAttributesAttribute([NotNull] string name)
        {
            Name = name;
        }
        [NotNull]
        public string Name
        {
            get;
            private set;
        }
    }
    [AttributeUsage(
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
        Inherited = true)]

    public sealed class HTMLAttributeValueAttribute : Attribute
    {
        public HTMLAttributeValueAttribute([NotNull] string name)
        {
            Name = name;
        }
        [NotNull]
        public string Name
        {
            get;
            private set;
        }
    }

    /*****************************************************************************
    * Purpose: Razor Attributes, indicates that a parameter or a method is Razor
    *           components. Use this attribute for custom wrappers
    * PseudoCode: 
    *           System.Web.WebPages.WebPageBase.RenderSection(String)
    *****************************************************************************/
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, Inherited = true)]

    public sealed class RazorSectionAttribute : Attribute { }
}
