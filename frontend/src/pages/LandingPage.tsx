import { Box, Button, Container, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import dietLogo from "../assets/diet-logo.png";

const videos = [
  "/videos/video1.mp4",
  "/videos/video2.mp4",
  "/videos/video3.mp4",
  "/videos/video4.mp4",
];

export default function LandingPage() {
  const navigate = useNavigate();
  const [currentVideoIndex, setCurrentVideoIndex] = useState(0);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    // Ustaw prędkość odtwarzania na 0.6x
    video.playbackRate = 0.6;

    const handleVideoEnd = () => {
      // Rozpocznij przyciemnienie
      setIsTransitioning(true);

      // Po 200ms zmień wideo
      setTimeout(() => {
        setCurrentVideoIndex((prev) => (prev + 1) % videos.length);
        // Po kolejnych 50ms rozpocznij rozjaśnianie
        setTimeout(() => setIsTransitioning(false), 50);
      }, 200);
    };

    video.addEventListener("ended", handleVideoEnd);
    return () => video.removeEventListener("ended", handleVideoEnd);
  }, []);

  useEffect(() => {
    const video = videoRef.current;
    if (video) {
      video.load();
      video.playbackRate = 0.6;
      video.play().catch((error) => {
        console.log("Autoplay prevented:", error);
      });
    }
  }, [currentVideoIndex]);

  return (
    <Box
      sx={{
        position: "relative",
        width: "100vw",
        height: "100vh",
        overflow: "hidden",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      {/* Video Background */}
      <Box
        component="video"
        ref={videoRef}
        autoPlay
        muted
        playsInline
        src={videos[currentVideoIndex]}
        sx={{
          position: "absolute",
          top: 0,
          left: 0,
          width: "100%",
          height: "100%",
          objectFit: "cover",
          filter: "blur(8px) brightness(0.7)",
          transform: "scale(1.1)",
          opacity: isTransitioning ? 0.3 : 1,
          transition: "opacity 0.25s ease-in-out",
        }}
      />

      {/* Overlay */}
      <Box
        sx={{
          position: "absolute",
          top: 0,
          left: 0,
          width: "100%",
          height: "100%",
          backgroundColor: "rgba(0, 0, 0, 0.4)",
        }}
      />

      {/* Content */}
      <Container
        maxWidth="md"
        sx={{
          position: "relative",
          zIndex: 1,
          textAlign: "center",
          color: "white",
        }}
      >
        <Box
          sx={{
            backgroundColor: "rgba(0, 0, 0, 0.6)",
            backdropFilter: "blur(10px)",
            borderRadius: 4,
            padding: { xs: 4, md: 6 },
            boxShadow: "0 8px 32px 0 rgba(0, 0, 0, 0.37)",
            border: "1px solid rgba(255, 255, 255, 0.18)",
          }}
        >
          {/* Logo */}
          <Box
            component="img"
            src={dietLogo}
            alt="Diet Zynzi Logo"
            sx={{
              width: { xs: 200, md: 250 },
              height: "auto",
              mb: 3,
              borderRadius: 3,
              boxShadow: "0 4px 12px rgba(0, 0, 0, 0.3)",
            }}
          />

          <Typography
            variant="h2"
            component="h1"
            fontWeight={800}
            gutterBottom
            sx={{
              fontSize: { xs: "2.5rem", md: "3.5rem" },
              textShadow: "2px 2px 4px rgba(0,0,0,0.5)",
            }}
          >
            Witaj w Diet Zynzi
          </Typography>

          <Typography
            variant="h5"
            sx={{
              mb: 4,
              fontSize: { xs: "1.2rem", md: "1.5rem" },
              textShadow: "1px 1px 3px rgba(0,0,0,0.5)",
              maxWidth: "800px",
              mx: "auto",
            }}
          >
            Nowoczesna aplikacja do planowania posiłków, zarządzania dietą i
            osiągania Twoich celów fitness. Personalizowane przepisy,
            inteligentne sugestie i wiele więcej.
          </Typography>

          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={2}
            justifyContent="center"
            sx={{ mt: 4 }}
          >
            <Button
              variant="contained"
              size="large"
              onClick={() => navigate("/login")}
              sx={{
                px: 4,
                py: 1.5,
                fontSize: "1.1rem",
                fontWeight: 600,
                backgroundColor: "primary.main",
                "&:hover": {
                  backgroundColor: "primary.dark",
                },
              }}
            >
              Zaloguj się
            </Button>
            <Button
              variant="outlined"
              size="large"
              onClick={() => navigate("/register")}
              sx={{
                px: 4,
                py: 1.5,
                fontSize: "1.1rem",
                fontWeight: 600,
                borderColor: "white",
                color: "white",
                "&:hover": {
                  borderColor: "white",
                  backgroundColor: "rgba(255, 255, 255, 0.1)",
                },
              }}
            >
              Zarejestruj się
            </Button>
          </Stack>
        </Box>
      </Container>
    </Box>
  );
}
