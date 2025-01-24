namespace ADHelper.Utility {
    public static class Alphatizer {
        public static string Alphatize(string dirty) {
            /*
            string mapDirty = "ŠŽšžŸÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöùúûüýÿ";
            string mapClean = "SZszYAAAAAACEEEEIIIIDNOOOOOUUUUYaaaaaaceeeeiiiidnooooouuuuyy";
            string clean = "";
            bool isCleanCharacter = true;
            for(int i = 0; i < dirty.Length; i++) {
                for(int k = 0; i < mapDirty.Length; k++) {
                    if (dirty[i] == mapDirty[k]) {
                        clean += mapClean[k];
                        isCleanCharacter = false;
                        break;
                    }
                }
                if(!isCleanCharacter) {
                    isCleanCharacter = true;
                } else {
                    clean += dirty[i]; 
                }
            }
            return clean;
            */
            return dirty;
        }
    }
}