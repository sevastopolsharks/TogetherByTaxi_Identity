namespace SevSharks.Identity.DataAccess.Models
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; set; }
    }
}
