import unittest
from Example import Example
from JJMasterData.Core.FormEvents.Args import *


class ExampleTest(unittest.TestCase):
    def test_something(self):
        example = Example()

        values = {}
        values["NumericField"] = -1

        errors = {}

        example.OnBeforeInsert(sender=self, args=FormBeforeActionEventArgs(Errors=errors, Values=values))

        self.assertEqual(errors["NumericField"], "Value needs to be greater than 0")


if __name__ == '__main__':
    unittest.main()
