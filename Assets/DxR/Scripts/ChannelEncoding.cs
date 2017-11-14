using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    public class ChannelEncoding 
    {
        public string channel;          // Name of channel.

        public string value;            // If not "undefined", use this value as constant value for channel.
        public string valueDataType;    // Type of value can be "float", "string", or "int".

        public string field;            // Name of field in data mapped to this channel.
        public string fieldDataType;    // Type of data field can be "quantitative", "temporal", "oridinal", or "nominal".

        public Scale scale;
        public Axis axis;

        public ChannelEncoding()
        {
            value = DxR.SceneObject.UNDEFINED;
            scale = null;
            axis = null;
        }
    }
}
