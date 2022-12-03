from dataclasses import dataclass


@dataclass
class FormBeforeActionEventArgs:
    Values: dict
    Errors: dict
