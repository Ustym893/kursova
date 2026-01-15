namespace EduFlow.Desktop.Views;

public class TeacherShellPage : TabbedPage
{
    public TeacherShellPage()
    {
        Title = "EduFlow (Teacher)";
        Children.Add(new TeacherDashboardPage { Title = "Dashboard" });
        Children.Add(new TeacherAssignmentsPage { Title = "Assignments" });
        Children.Add(new TeacherStudentsPage { Title = "Students" });
    }
}