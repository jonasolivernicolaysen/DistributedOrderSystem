function RegisterPage() {
    return (
        <div>
            <h1>Register</h1>

            <input type="text" placeholder="First Name" />
            <input type="text" placeholder="Last Name" />
            <input type="text" placeholder="Username" />

            <input type="password" placeholder="Password" />

            <input type="password" placeholder="Repeat password" />

            <button type="button">Register</button>
        </div>
    );
}

export default RegisterPage;