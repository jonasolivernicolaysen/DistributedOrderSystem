import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { SubmitEvent } from "react";
import { toast } from "react-toastify"
import { apiFetch } from "../services/api"

function RegisterPage() {
    // state
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const navigate = useNavigate();

    // functions

    const handleRegister = async (e: SubmitEvent) => {
        // prevent page from reloading
        e.preventDefault(); 

        if (hasErrors) {
            return;
        }

        try {
            const response = await apiFetch(
                "http://localhost:7144/api/auth/register",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        username,
                        email,
                        password
                    })
                });

            const data = await response.json();
            if (!response.ok) {
                toast.error(data.detail);
                return;
            }

            toast.success("Registration successful");

            navigate("/login")

        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    const usernameRegex = /^[A-Za-z][A-Za-z0-9_]{3,19}$/;
    const usernameError = username.length > 0 && !usernameRegex.test(username)
        ? "Username must start with a letter and be 4-20 characters long. Only letters, numbers, and underscores are allowed."
        : "";

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const emailError = email.length > 0 && !emailRegex.test(email)
        ? "Invalid email address"
        : "";
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'"\\|,.<>/?]).{8,}$/;
    const passwordError = password.length > 0 && !passwordRegex.test(password)
        ? "Password must be at least 8 characters and contain uppercase, lowercase, a number, and a special character."
        : "";

    const hasErrors = usernameError || emailError || passwordError;

    const fieldClass = (error: string) => `form-control ${error ? "is-invalid" : ""}`;

    // UI

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-4">

                    <div className="card shadow">
                        <div className="card-body">

                            <h2 className="text-center mb-4">
                                Register
                            </h2>

                            <form onSubmit={handleRegister}>
                                <div className="mb-3">
                                    <input
                                        className={fieldClass(usernameError)}
                                        value={username}
                                        onChange={(e) => setUsername(e.target.value)}
                                        type="text"
                                        placeholder="Username"
                                        required
                                    />

                                    {usernameError && (
                                        <div className="invalid-feedback d-block">
                                            {usernameError}
                                        </div>
                                    )}
                                </div>

                                <div className="mb-3">
                                    <input
                                        className={fieldClass(emailError)}
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        type="email"
                                        placeholder="Email"
                                        required
                                    />

                                    {emailError && (
                                        <div className="invalid-feedback d-block">
                                            {emailError}
                                        </div>
                                    )}
                                </div>

                                <div className="mb-3">
                                    <input
                                        className={fieldClass(passwordError)}
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        type="password"
                                        placeholder="Password"
                                        required
                                    />

                                    {passwordError && (
                                        <div className="invalid-feedback d-block">
                                            {passwordError}
                                        </div>
                                    )}
                                </div>

                                <button
                                    type="submit"
                                    className="btn btn-primary w-100"
                                >
                                    Register
                                </button>
                            </form>

                            <p className="text-center text-muted small mt-3 mb-0">
                                Already have an account? <a href="/">Login here</a>
                            </p>

                        </div>
                    </div>

                </div>
            </div>
        </div>
    );
}

export default RegisterPage;