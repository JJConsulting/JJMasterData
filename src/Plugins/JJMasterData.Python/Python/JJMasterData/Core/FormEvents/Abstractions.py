from .Args import FormBeforeActionEventArgs
from .Args import FormAfterActionEventArgs
from .Args import MetadataLoadEventArgs


# noinspection PyPep8Naming
class BaseFormEvent:
    def OnAfterDelete(self, sender, args: FormAfterActionEventArgs):
        pass

    def OnAfterImport(self, sender, args: FormAfterActionEventArgs):
        pass

    def OnAfterInsert(self, sender, args: FormAfterActionEventArgs):
        pass

    def OnAfterUpdate(self, sender, args: FormAfterActionEventArgs):
        pass

    def OnBeforeDelete(self, sender, args: FormBeforeActionEventArgs):
        pass

    def OnBeforeImport(self, sender, args: FormBeforeActionEventArgs):
        pass

    def OnBeforeInsert(self, sender, args: FormBeforeActionEventArgs):
        pass

    def OnBeforeUpdate(self, sender, args: FormBeforeActionEventArgs):
        pass

    def OnMetadataLoad(self, sender, args: MetadataLoadEventArgs):
        pass
