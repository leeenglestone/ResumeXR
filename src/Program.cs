using StereoKit;
using StereoKit.Framework;

namespace StereoKitResume;

class Program
{
    static float z = -0.4f;
    static bool showPositions = false;
    static Vec3 origin = new Vec3();
    static Quat lookForward = Quat.LookDir(0, 0, 1);

    // Original positions
    static Pose originalAboutPose = new Pose(0.5f, 0.35f, z, lookForward);
    static Pose originalEducationPose = new Pose(-0.2f, 0.075f, z, lookForward);
    static Pose originalExperiencePose = new Pose(-0.2f, 0.35f, z, lookForward);
    static Pose originalGoodBusinessBooksPose = new Pose(-0.2f, -0.08f, z, lookForward);
    static Pose originalHeaderPose = new Pose(0.15f, 0.35f, z, lookForward);
    static Pose originalMyBookPose = new Pose(0.5f, -0.06f, z, lookForward);
    static Pose originalProjectsPose = new Pose(0.145f, -0.25f, z, lookForward);
    static Pose originalLatestPostPose = new Pose(0.15f, 0.145f, z, lookForward);

    // Keeps track of positions
    static Pose aboutPose;
    static Pose educationPose;
    static Pose experiencePose;
    static Pose goodBusinessBooksPose;
    static Pose headerPose;
    static Pose myBookPose;
    static Pose projectsPose;
    static Pose latestPostPose;

    static PassthroughFBExt Passthrough;

    static void Main(string[] args)
    {
        // Initialize StereoKit
        SKSettings settings = new SKSettings
        {
            appName = "StereoKitResume",
            assetsFolder = "Assets",
        };

        Passthrough = SK.AddStepper(new PassthroughFBExt());

        if (!SK.Initialize(settings))
            return;

        Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
        Material floorMaterial = new Material("floor.hlsl");
        floorMaterial.Transparency = Transparency.Blend;

        Sprite aboutSprite = Sprite.FromFile("about.png");
        Sprite educationSprite = Sprite.FromFile("education.png");
        Sprite experienceSprite = Sprite.FromFile("experience.png");
        Sprite goodBusinessBooksSprite = Sprite.FromFile("good-business-books.png");
        Sprite headerSprite = Sprite.FromFile("header.png");
        Sprite myBookSprite = Sprite.FromFile("my-book.png");
        Sprite projectsSprite = Sprite.FromFile("projects.png");
        Sprite latestPostSprite = Sprite.FromFile("latest-post.png");

        ResetPositions();

        // Core application loop
        SK.Run(() =>
        {
            if (!Passthrough.EnabledPassthrough)
            {
                if (SK.System.displayType == Display.Opaque)
                    Default.MeshCube.Draw(floorMaterial, floorTransform);
            }

            DrawImage("About", ref aboutPose, aboutSprite);
            DrawImage("Education", ref educationPose, educationSprite);
            DrawImage("Experience", ref experiencePose, experienceSprite);
            DrawImage("Good Business Books", ref goodBusinessBooksPose, goodBusinessBooksSprite);
            DrawImage("Header", ref headerPose, headerSprite);
            DrawImage("My Book", ref myBookPose, myBookSprite);
            DrawImage("Projects", ref projectsPose, projectsSprite);
            DrawImage("Latest Post", ref latestPostPose, latestPostSprite);

            DrawHandMenu(Handed.Left);
        });
    }

    private static void DrawImage(string id, ref Pose pose, Sprite sprite)
    {
        float factor = 4000;

        UI.WindowBegin(id, ref pose, UIWin.Empty, UIMove.FaceUser);

        UI.Image(sprite, new Vec2(sprite.Width / factor, sprite.Height / factor));

        if(showPositions)
            UI.Label($"{pose.position.x}, {pose.position.y}, {pose.position.z}");

        UI.WindowEnd();
    }

    static bool HandFacingHead(Handed handed)
    {
        Hand hand = Input.Hand(handed);
        if (!hand.IsTracked)
            return false;

        Vec3 palmDirection = (hand.palm.Forward).Normalized;
        Vec3 directionToHead = (Input.Head.position - hand.palm.position).Normalized;

        return Vec3.Dot(palmDirection, directionToHead) > 0.5f;
    }

    public static void DrawHandMenu(Handed handed)
    {
        if (!HandFacingHead(handed))
            return;

        // Decide the size and offset of the menu
        Vec2 size = new Vec2(4, 16);
        float offset = handed == Handed.Left ? -2 - size.x : 2 + size.x;

        // Position the menu relative to the side of the hand
        Hand hand = Input.Hand(handed);
        Vec3 at = hand[FingerId.Little, JointId.KnuckleMajor].position;
        Vec3 down = hand[FingerId.Little, JointId.Root].position;
        Vec3 across = hand[FingerId.Index, JointId.KnuckleMajor].position;

        Pose menuPose = new Pose(
            at,
            Quat.LookAt(at, across, at - down) * Quat.FromAngles(0, handed == Handed.Left ? 90 : -90, 0));
        menuPose.position += menuPose.Right * offset * U.cm;
        menuPose.position += menuPose.Up * (size.y / 2) * U.cm;

        // And make a hand menu!
        UI.WindowBegin("HandMenu", ref menuPose, size * U.cm, UIWin.Empty);
        
        if (UI.Button("Reset positions"))
        {
            ResetPositions();            
        }

        UI.Toggle("Show positions", ref showPositions);        
        
        if (UI.Button("Exit"))
            SK.Quit();

        UI.WindowEnd();
    }

    static void ResetPositions()
    {
        aboutPose = originalAboutPose;
        educationPose = originalEducationPose;
        experiencePose = originalExperiencePose;
        goodBusinessBooksPose = originalGoodBusinessBooksPose;
        headerPose = originalHeaderPose;
        myBookPose = originalMyBookPose;
        projectsPose = originalProjectsPose;
        latestPostPose = originalLatestPostPose;
    }
}