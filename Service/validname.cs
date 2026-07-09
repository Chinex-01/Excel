namespace nameService
{
    public class validname
    {
        public bool IsValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (name.Contains("@") || name.Contains("#") || name.Contains("1") || name.Contains("()"))
            {
                return false;
            }

            return true;
        }
    }
}
