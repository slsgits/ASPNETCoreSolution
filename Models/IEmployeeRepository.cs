namespace EmployeeManagement.Models
{
    // This interface defines the contract for an employee repository, which includes methods for CRUD operations on Employee entities.
    // Implementations of this interface can provide different data storage mechanisms, such as in-memory storage or database access.
    // The methods defined in this interface allow for retrieving, adding, updating, and deleting Employee records.
    // The use of an interface allows for dependency injection and promotes loose coupling between the application components.
    // This interface can be implemented by classes that interact with different data sources, such as a SQL database or an in-memory collection.
    // The methods in this interface return Employee objects or collections of Employee objects, allowing for easy manipulation and retrieval of employee data.
    // The interface can be extended in the future to include additional methods for more complex queries or operations on Employee entities.
    // Overall, this interface serves as a blueprint for managing employee data in a consistent and flexible manner across the application.
    // One can change methods to async methods to improve performance and scalability in a real-world application.
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int Id);
        IEnumerable<Employee> GetAllEmployees();
        Employee Add(Employee employee);
        Employee Update(Employee employeeChanges);
        Employee Delete(Employee employee);
        void DeleteEmployeePhotos(List<EmployeePhoto> photos);
    }
}
