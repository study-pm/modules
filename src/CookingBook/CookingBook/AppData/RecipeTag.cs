//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CookingBook.AppData
{
    using System;
    using System.Collections.Generic;
    
    public partial class RecipeTag
    {
        public int RecipeTagID { get; set; }
        public int RecipeID { get; set; }
        public int TagID { get; set; }
    
        public virtual Recipe Recipe { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
