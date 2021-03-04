Feature: Paint
	Test Paint

Scenario: Draw Triangle
	Given I draw a traingle
	Then image with 3 sides is found

Scenario: Draw Rectangle
	Given I draw a rectangle
	Then image with 4 sides is found