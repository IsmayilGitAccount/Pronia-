namespace ProniaApplication.Areas.ViewModels
{
    public class PaginateVM<T>
    {
        public double TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public List<T> Items { get; set; }
    }
}
