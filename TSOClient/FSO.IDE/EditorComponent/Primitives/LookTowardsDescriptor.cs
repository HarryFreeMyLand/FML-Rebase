﻿using FSO.Files.Formats.IFF.Chunks;
using FSO.IDE.EditorComponent.Model;
using FSO.IDE.EditorComponent.OperandForms;
using FSO.IDE.EditorComponent.OperandForms.DataProviders;
using FSO.SimAntics.Engine.Scopes;
using FSO.SimAntics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSO.IDE.EditorComponent.Primitives
{
    public class LookTowardsDescriptor : PrimitiveDescriptor
    {
        public override PrimitiveGroup Group { get { return PrimitiveGroup.Sim; } }
        public override PrimitiveReturnTypes Returns { get { return PrimitiveReturnTypes.Done; } }
        public override Type OperandType { get { return typeof(VMLookTowardsOperand); } }

        public override string GetBody(EditorScope scope)
        {
            var op = (VMLookTowardsOperand)Operand;
            var result = new StringBuilder();

            result.Append(EditorScope.Behaviour.Get<STR>(216).GetString((int)op.Mode));
            return result.ToString();
        }

        public override void PopulateOperandView(BHAVEditor master, EditorScope escope, TableLayoutPanel panel)
        {
            panel.Controls.Add(new OpLabelControl(master, escope, Operand, 
                new OpStaticTextProvider("Turns either the Avatar's body or head towards/away from either the Stack Object or the camera.")));
            panel.Controls.Add(new OpComboControl(master, escope, Operand, "Mode:", "Mode", new OpStaticNamedPropertyProvider(EditorScope.Behaviour.Get<STR>(216))));
        }
    }
}
