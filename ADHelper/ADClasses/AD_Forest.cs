using System;
using System.DirectoryServices.ActiveDirectory;

namespace ADHelper.ADClasses
{
    class AD_Forest
    {
        Forest forest = Forest.GetCurrentForest();
    }
}
