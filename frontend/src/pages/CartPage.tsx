import { useEffect, useState } from "react"; 

function CartPage() {
    // state

    interface CartItem {
        productId: string,
        name: string,
        description: string,
        price: number
    }
    const [products, setProducts] = useState<CartItem[]>([]);

    // functions

    // get cart items from backend
    const getCartItems = async () => {
        try {
            // get token
            const token = localStorage.getItem("token");

            // send request to authservice getcart endpoint
            const response = await fetch(
                "https://localhost:7144/api/orders/cart",
                {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });

            if (!response.ok) {
                alert(`Request failed: ${response.status}`);
                return;
            }
            const data = await response.json();

            // fetches the items instead of the cart 
            console.log(data)
            console.log(data.items)
            setProducts(data.items);

        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    }

    useEffect(() => {
        getCartItems();
    }, [])

    // return html
    return (
        <div>
            <h1>Cart</h1>

            <div className="container mt-4">
                <div className="row">
                    {products.map((product: CartItem) => (
                        <div
                            key={product.productId}
                            className="col-md-4 mb-4"
                        >
                            <div
                                className="card h-100 shadow-sm"
                                style={{ cursor: "pointer" }}
                                onClick={() => console.log(product.productId)}
                            >
                                <div className="card-body">
                                    <h5 className="card-title">
                                        {product.name}
                                    </h5>

                                    <p className="card-text">
                                        {product.description}
                                    </p>
                                </div>

                                <div className="card-footer">
                                    <strong>{product.price}</strong>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    )
}

export default CartPage;