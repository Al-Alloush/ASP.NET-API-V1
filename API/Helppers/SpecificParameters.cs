using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helppers
{
    public class SpecificParameters
    {

        private const int MaxPageSize = 50;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PagesCount { get; set; }
        public string Sort { get; set; }
        private string _search;

        public string Search
        {
            get => _search;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _search = value.ToLower();
                };
            }
        }

        // for Blogs
        public int? CategoryId { get; set; }

        // for Users, Email / UserName
        public string SearchInColumnName { get; set; }
        public string Role { get; set; }


        public SpecificParameters()
        {
            this.PageIndex = 1;
            this.PageSize = MaxPageSize;
        }
        public SpecificParameters(int pageIndex, int pageSize)
        {
            this.PageIndex = pageIndex < 1 ? 1 : pageIndex;
            this.PageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;
        }
    }
}
