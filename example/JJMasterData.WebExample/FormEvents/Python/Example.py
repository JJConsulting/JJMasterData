from JJMasterData.Core.FormEvents.Abstractions import BaseFormEvent
from JJMasterData.Core.FormEvents.Args import *


class Example(BaseFormEvent):

    def OnBeforeInsert(self, sender, args: FormBeforeActionEventArgs):
        if int(args.Values["NumericField"]) < 0:
            args.Errors["NumericField"] = "Value needs to be greater than 0"

    def OnMetadataLoad(self, sender, args: MetadataLoadEventArgs):
        args.Metadata.Form.SubTitle = "Hello from IronPython runtime."
