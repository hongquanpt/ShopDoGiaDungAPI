namespace ShopDoGiaDungAPI.DTO
{
    public class PermissionDto
    {
        public string FunctionCode { get; set; }
        public string ActionCode { get; set; }

        public bool Equals(PermissionDto other)
        {
            if (other == null)
                return false;

            return this.FunctionCode == other.FunctionCode && this.ActionCode == other.ActionCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PermissionDto);
        }

        public override int GetHashCode()
        {
            return (FunctionCode + ActionCode).GetHashCode();
        }
    }
}
