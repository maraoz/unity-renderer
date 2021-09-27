using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IProfileCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the profile card is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; set; }

    /// <summary>
    /// Fill the model and updates the profile card with this data.
    /// </summary>
    /// <param name="model">Data to configure the profile card.</param>
    void Configure(ProfileCardComponentModel model);

    /// <summary>
    /// Set the profile picture.
    /// </summary>
    /// <param name="newPicture">Profile picture (sprite).</param>
    void SetProfilePicture(Sprite newPicture);

    /// <summary>
    /// Set the profile picture.
    /// </summary>
    /// <param name="newPicture">Profile picture (texture).</param>
    void SetProfilePicture(Texture2D newPicture);

    /// <summary>
    /// Set the profile name.
    /// </summary>
    /// <param name="newName">Profile name.</param>
    void SetProfileName(string newName);

    /// <summary>
    /// Set the profile address. It will only show the last 4 caracteres.
    /// </summary>
    /// <param name="newAddress">Profile address.</param>
    void SetProfileAddress(string newAddress);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class ProfileCardComponentView : BaseComponentView, IProfileCardComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal ImageComponentView profileImage;
    [SerializeField] internal TMP_Text profileName;
    [SerializeField] internal TMP_Text profileAddress;

    [Header("Configuration")]
    [SerializeField] internal ProfileCardComponentModel model;

    public Button.ButtonClickedEvent onClick
    {
        get
        {
            if (button == null)
                return null;

            return button.onClick;
        }
        set
        {
            model.onClick = value;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    value?.Invoke();
                });
            }
        }
    }

    public override void PostInitialization() { Configure(model); }

    public void Configure(ProfileCardComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetProfilePicture(model.profilePicture);
        SetProfileName(model.profileName);
        SetProfileAddress(model.profileAddress);
        onClick = model.onClick;
    }

    public override void Dispose()
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
    }

    public void SetProfilePicture(Sprite newPicture)
    {
        model.profilePicture = newPicture;

        if (profileImage == null)
            return;

        profileImage.SetImage(newPicture);
    }

    public void SetProfilePicture(Texture2D newPicture)
    {
        Sprite newPictureSprite = Sprite.Create(newPicture, new Rect(0, 0, newPicture.width, newPicture.height), new Vector2(0.5f, 0.5f));

        model.profilePicture = newPictureSprite;

        if (profileImage == null)
            return;

        profileImage.SetImage(newPictureSprite);
    }

    public void SetProfileName(string newName)
    {
        model.profileName = newName;

        if (profileName == null)
            return;

        profileName.text = !string.IsNullOrEmpty(newName) ? newName : string.Empty;
    }

    public void SetProfileAddress(string newAddress)
    {
        model.profileAddress = newAddress;

        if (profileAddress == null)
            return;

        if (!string.IsNullOrEmpty(newAddress))
            profileAddress.text = newAddress.Length >= 4 ? $"#{newAddress.Substring(newAddress.Length - 4, 4)}" : $"#{newAddress}";
        else
            profileAddress.text = string.Empty;
    }

    public void SetLoadingIndicatorVisible(bool isVisible) { profileImage.SetLoadingIndicatorVisible(isVisible); }
}