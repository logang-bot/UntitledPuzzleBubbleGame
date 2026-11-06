using System;

namespace Game.Results
{
    public struct ResultButtonsSpec
    {
        public string PrimaryLabel;
        public Action OnPrimary;
        public string SecondaryLabel;
        public Action OnSecondary;
    }
}
