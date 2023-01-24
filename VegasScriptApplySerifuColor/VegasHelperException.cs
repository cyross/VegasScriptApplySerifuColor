using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VegasScriptApplySerifuColor
{
    internal class VegasHelperException : Exception { }
    internal class VegasHelperNotFoundTrackException : VegasHelperException { }
    internal class VegasHelperTrackUnselectedException : VegasHelperException { }
    internal class VegasHelperNoneEventsException : VegasHelperException { }
    internal class VegasHelperNotFoundOFXParameterException : VegasHelperException { }
    internal class VegasHelperNotFoundJimakuPrefixException : VegasHelperException { }
}
