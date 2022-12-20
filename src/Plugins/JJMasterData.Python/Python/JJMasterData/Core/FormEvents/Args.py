from dataclasses import dataclass


@dataclass
class FormAfterActionEventArgs:
    Values: dict
    UrlRedirect: str


@dataclass
class FormBeforeActionEventArgs:
    Values: dict
    Errors: dict


@dataclass
class MetadataLoadEventArgs:
    Metadata: any
