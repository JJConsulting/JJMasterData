from dataclasses import dataclass


@dataclass
class FormAfterActionEventArgs:
    Values: dict
    UrlRedirect: str
