namespace EduFlow.Desktop.Views;

public partial class RolePage : ContentPage
{
    public RolePage()
    {
        InitializeComponent();
    }

    private async void OnTeacher(object sender, EventArgs e)
        => await Navigation.PushAsync(new LoginPage("TEACHER"));

    private async void OnStudent(object sender, EventArgs e)
        => await Navigation.PushAsync(new LoginPage("STUDENT"));
}