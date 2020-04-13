Feature: Extended Simple Container
	In order to write robust flexible apps
	As an app developer
	I want to be able to use inversion of control container

Scenario: Resolving a dependency that has been registered with a named parameter should yield correct value 
	When The container is created
	And The dependency with named parameter is registered in transient fashion
	And The dependency is resolved with value '5' for named parameter
	Then Actual value of parameter inside the named dependency is '5'

Scenario: Resolving a dependency that has been registered with a typed parameter should yield correct value 
	When The container is created
	And The dependency with typed parameter is registered in transient fashion
	And The dependency is resolved with value 6 for typed parameter
	Then Actual value of parameter inside the typed dependency is 6

Scenario: Resolving multiple same type dependencies should yield the whole collection
	When The container is created
	And The same type dependency is registered in instance fashion using first instance
	And The same type dependency is registered in instance fashion using second instance
	And The collection of dependencies is resolved
	Then The collection of dependencies is equivalent to the collection of instances

Scenario: Resolving a dependency that has been registered with a typed parameter in singleton fashion multiple times should yield same value
	When The container is created
	And The dependency with named parameter is registered in singleton fashion
	Then Multiple dependency resolutions yield same value
