namespace Flowers_Yasuo.MyBase
{
    #region 

    using Flowers_Yasuo.MyCommon;

    using System;

    #endregion

    public class MyChampions
    {
        public MyChampions()
        {
            try
            {
                Initializer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyChampions." + ex);
            }
        }

        public void Initializer()
        {
            try
            {
                MySpellManager.Initializer();
                MyMenuManager.Initializer();
                MyEventManager.Initializer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyChampions.Initializer" + ex);
            }
        }
    }
}
